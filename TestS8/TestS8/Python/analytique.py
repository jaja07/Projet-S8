import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import datetime
import tarfile
from sklearn.metrics import confusion_matrix, accuracy_score
from config import dataframe
import zipfile

def analytical(t0_run_mof, steps_mof):
    #extraction de l'archive
    fichier_zip = "data.zip"
    fichier_zip2 = "data2.zip"
    #raise FileNotFoundError(f"Fichier ZIP introuvable: {fichier_zip}")
    if not os.path.exists(fichier_zip):
        with zipfile.ZipFile(fichier_zip2, 'r') as zip_ref2:
            zip_ref2.extractall()
    else:
        with zipfile.ZipFile(fichier_zip, 'r') as zip_ref:
            zip_ref.extractall()

    pathfile = 'data_anonymous'
    # dataframe reflist
    reflist=pd.DataFrame()    
    files=os.listdir(pathfile) 
    for file in files:
        if file.startswith('reflist_'):
            temp=pd.read_csv(os.path.join(pathfile,file),sep=',').reset_index(drop=True)[['Epc']]
            temp['refListId']=file.split('.')[0]
            reflist = pd.concat([reflist, temp])
    reflist=reflist.rename(columns={'refListId':'refListId_actual'})
    reflist['refListId_actual']=reflist['refListId_actual'].apply(lambda x:int(x[8:]))
    Q_refListId_actual=reflist.groupby('refListId_actual')['Epc'].nunique().rename('Q refListId_actual').reset_index(drop=False)
    reflist=pd.merge(reflist,Q_refListId_actual,on='refListId_actual',how='left')
    
    #dataframe tags
    tags=pd.DataFrame()
    files=os.listdir(pathfile)
    for file in files:
        if file.startswith('ano_APTags'):
            temp=pd.read_csv(os.path.join(pathfile,file),sep=',')
            tags=pd.concat([tags,temp])
    tags['LogTime']=pd.to_datetime (tags['LogTime'] ,format='%Y-%m-%d-%H:%M:%S') 
    tags['TimeStamp']=tags['TimeStamp'].astype(float)
    tags['Rssi']=tags['Rssi'].astype(float)
    tags=tags.drop(['Reader','EmitPower','Frequency'],axis=1).reset_index(drop=True)
    tags=tags[['LogTime', 'Epc', 'Rssi', 'Ant']]
    Ant_loc=pd.DataFrame({'Ant':[1,2,3,4],'loc':['in','in','out','out']})
    tags=pd.merge(tags,Ant_loc,on=['Ant'])
    tags=tags.sort_values('LogTime').reset_index(drop=True)
    
    # dataframe timing
    file='ano_supply-process.2019-11-07-CUT.csv'
    timing=pd.read_csv(os.path.join(pathfile,file),sep=',')
    timing['file']=file
    timing['date']=pd.to_datetime(timing['date'],format='%d/%m/%Y %H:%M:%S,%f')
    timing['Start']=pd.to_datetime(timing['ciuchStart'],format='%d/%m/%Y %H:%M:%S,%f')
    timing['Stop']=pd.to_datetime(timing['ciuchStop'],format='%d/%m/%Y %H:%M:%S,%f')
    timing['timestampStart']=timing['timestampStart'].astype(float)
    timing['timestampStop']=timing['timestampStop'].astype(float)
    timing=timing.sort_values('date')
    timing.loc[:,'refListId']=timing.loc[:,'refListId'].apply(lambda x:int(x[8:]))
    timing=timing[['refListId', 'Start', 'Stop']]
    
    # Fenêtre de décision: full Window  
    # Start_up starts upstream Start, half way in between the previous stop and the actual start
    timing[['Stop_last']]=timing[['Stop']].shift(1)
    timing[['refListId_last']]=timing[['refListId']].shift(1)
    timing['Startup']=timing['Start'] - (timing['Start'] - timing['Stop_last'])/2
    # timing start: 10sec before timing
    timing.loc[0,'refListId_last']=timing.loc[0,'refListId']
    timing.loc[0,'Startup']=timing.loc[0,'Start']-datetime.timedelta(seconds=10)
    timing.loc[0,'Stop_last']=timing.loc[0,'Startup']-datetime.timedelta(seconds=10)
    timing['refListId_last']=timing['refListId_last'].astype(int)
    timing['Stopdown']= timing['Startup'].shift(-1)
    timing.loc[len(timing)-1,'Stopdown']=timing.loc[len(timing)-1,'Stop']+datetime.timedelta(seconds=10)
    timing=timing[['refListId', 'refListId_last','Startup', 'Start','Stop','Stopdown']]

    t0_run=timing[timing['refListId']==t0_run_mof] [['Startup']]
    t0_run=t0_run.rename(columns={'Startup':'t0_run'})
    t0_run=t0_run.groupby('t0_run').size().cumsum().rename('run').reset_index(drop=False)
    t0_run=t0_run.sort_values('t0_run')
   
    # t0_run = a new run starts when box 0 shows up
    # t0_run: c'est l'instant à partir duquel commence un run, chaque run commence avec la boxe 0
    # run : id du run
    # each row in timing is merged with a last row in t0_run where t0_run (ciuchstart) <= timing (ciuchstart)
    timing=pd.merge_asof(timing,t0_run,left_on='Startup',right_on='t0_run', direction='backward')
    timing=timing.sort_values('Stop')
    timing=timing[['run', 'refListId', 'refListId_last', 'Startup','Start','Stop','Stopdown','t0_run']]
    
    
    #  full window (Startup > Stopdown) is sliced in smaller slices
    # Startup > Start: 11 slices named up_0, up_1, ..., up_10
    # Start > Stop: 11 slices named mid_0, mid_1, ... mid_10
    # Stop > Stopdown: 11 slices names down_0, down_1, ... down_10
    slices=pd.DataFrame()
    for i, row in timing.iterrows(): 
        ciuchStartup=row['Startup']
        ciuchStart=row['Start']
        ciuchStop=row['Stop']
        ciuchStopdown=row['Stopdown']
        steps=steps_mof
    #     
        up=pd.DataFrame(index=pd.date_range(start=ciuchStartup, end=ciuchStart,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        up.index=['up_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, up])    
  
        mid=pd.DataFrame(index=pd.date_range(start=ciuchStart, end=ciuchStop,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        mid.index=['mid_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, mid])
#     
        down=pd.DataFrame(index=pd.date_range(start=ciuchStop, end=ciuchStopdown,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        down.index=['down_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, down])

    slices=slices.reset_index(drop=False).rename(columns={'index':'slice_id'})
    # 
    timing_slices=pd.merge_asof(slices,timing,left_on='slice',right_on='Startup',direction='backward')
    timing_slices=timing_slices[['run', 'refListId', 'refListId_last','slice_id','slice',  \
                             'Startup', 'Start', 'Stop', 'Stopdown','t0_run']]
    
    
    #  full window (Startup > Stopdown) is sliced in smaller slices
    # Startup > Start: 11 slices named up_0, up_1, ..., up_10
    # Start > Stop: 11 slices named mid_0, mid_1, ... mid_10
    # Stop > Stopdown: 11 slices names down_0, down_1, ... down_10
    slices=pd.DataFrame()
    for i, row in timing.iterrows(): 
        ciuchStartup=row['Startup']
        ciuchStart=row['Start']
        ciuchStop=row['Stop']
        ciuchStopdown=row['Stopdown']
        steps=4
#     
        up=pd.DataFrame(index=pd.date_range(start=ciuchStartup, end=ciuchStart,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        up.index=['up_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, up])    
  
        mid=pd.DataFrame(index=pd.date_range(start=ciuchStart, end=ciuchStop,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        mid.index=['mid_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, mid])
#     
        down=pd.DataFrame(index=pd.date_range(start=ciuchStop, end=ciuchStopdown,periods=steps)).reset_index(drop=False).rename(columns={'index':'slice'})
        down.index=['down_'+str(x) for x in range(steps)]
        slices= pd.concat([slices, down])

    slices=slices.reset_index(drop=False).rename(columns={'index':'slice_id'})
    timing_slices=pd.merge_asof(slices,timing,left_on='slice',right_on='Startup',direction='backward')
    timing_slices=timing_slices[['run', 'refListId', 'refListId_last','slice_id','slice', 'Startup', 'Start', 'Stop', 'Stopdown','t0_run']]
    
    
    #Fusion des dataframes tags et timing_slices: df_timing_slices
    
    # merge between tags and timing
    # merge_asof needs sorted df > df_ref
    df = tags
    df=df[(tags['LogTime']>=timing['Startup'].min()) & (df['LogTime']<=timing['Stopdown'].max())]
    df=df.sort_values('LogTime')
    df_timing_slices=pd.merge_asof(df,timing_slices,left_on=['LogTime'],right_on=['slice'],direction='backward')
    df_timing_slices=df_timing_slices.dropna()
    df_timing_slices=df_timing_slices.sort_values('slice').reset_index(drop=True)
    df_timing_slices['window_run_id']  = df_timing_slices['refListId'].astype(str) +'_'+ df_timing_slices['run'].astype(str)
    df_timing_slices=df_timing_slices[['run', 'window_run_id', 'Epc','refListId', 'refListId_last', 'Startup','slice_id','slice','LogTime','Start','Stop', 'Stopdown', 'Rssi', 'loc','t0_run']]
    
    # 9eme partie
    
    runs_out=df_timing_slices .groupby('run')['refListId'].nunique().rename('Q refListId').reset_index(drop=False)
    runs_out[runs_out['Q refListId']!=10]
    
    # 10eme partie
    
    current_last_windows=timing_slices.drop_duplicates(['run','refListId','refListId_last'])
    current_last_windows=current_last_windows[['run','refListId','refListId_last','Stop']].reset_index(drop=True)
    current_last_windows[:1]
    
    #11eme partie
    
    # runs 16 23 32 40 have missing boxes: discarded
    # also run 1 is the start, no previous box: discarded
    # run 18: box 0 run at the end
    # 
    timing=timing[~timing['run'].isin([1,18,16,23,32,40])]
    timing_slices=timing_slices[~timing_slices['run'].isin([1,18,16,23,32,40])]
    df_timing_slices=df_timing_slices[~df_timing_slices['run'].isin([1,18,16,23,32,40])]
    
    #12eme partie
    df_timing_slices=df_timing_slices.sort_values(['LogTime','Epc'])
    #
    #13eme partie
    len(timing),len(timing_slices), len(df_timing_slices)
    
    #14eme partie
    
    # df_timing_slices['dt']=
    df_timing_slices['dt']=(df_timing_slices['LogTime']-df_timing_slices['t0_run']).apply(lambda x:x.total_seconds())
    
    #15eme partie
    rssi_threshold=-110
    
    '''
    if dataframe:
        df_timing_slices = dataframe['df_timing_slices']
        timing_slices = dataframe['timing_slices']
        reflist = dataframe['reflist']
        
    else:
        df_timing_slices = pd.read_excel("df_timing_slices.xlsx")
        timing_slices = pd.read_excel("timing_slices.xlsx")
        reflist = pd.read_excel("reflist.xlsx")
        '''
    
    ana = df_timing_slices.groupby(['Epc', 'window_run_id', 'slice_id', 'loc']) ['Rssi'].max().unstack('loc', fill_value =- 110).reset_index(drop=False)
    order=pd.DataFrame(timing_slices['slice_id'].unique(), columns=['slice_id'])
    order['order']=order.index

    ana=pd.merge(ana, order, on='slice_id', how='left')
    ana = ana [['Epc', 'window_run_id', 'slice_id', 'in', 'out', 'order']]
    
    # last subslice_id with out>in
    ana_out =ana [ ana['out']>ana['in'] ].sort_values(['Epc', 'window_run_id', 'order'], ascending=False).drop_duplicates(['Epc', 'window_run_id'])
    
    # first subslice_id with in<out
    ana_in =ana [ ana['in']>ana['out'] ].sort_values(['Epc', 'window_run_id', 'order'], ascending=True).drop_duplicates(['Epc', 'window_run_id'])

    ana = pd.merge(ana_in, ana_out, on=['Epc', 'window_run_id'], suffixes=['_IN', '_OUT'], how='inner').sort_values(['Epc', 'window_run_id'])
    ana = pd.merge(ana, reflist, on='Epc', how='left')

    ana['pred_ana_bool']= ana['window_run_id'].apply(lambda x:x.split('_')[0]).astype('int64') == ana['refListId_actual']
  
    vrai=ana[ana['pred_ana_bool']==True]
    accuracy= vrai.shape[0]/ana.shape[0]
    return accuracy





