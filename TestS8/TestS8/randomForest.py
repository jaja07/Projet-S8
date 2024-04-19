import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import datetime
import zipfile
from sklearn.metrics import confusion_matrix, accuracy_score
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import MinMaxScaler
from sklearn.model_selection import train_test_split, cross_val_score



def random_forest (n_estimators_rm, max_depth_rm,  min_samples_split_rm, min_samples_leaf_rm, bootstrap_rm):

    #extraction de l'archive
    fichier_zip = "data_anonymous.zip"
    repertoire_destination = "data_anonymous"

    if not os.path.exists(fichier_zip):
        raise FileNotFoundError(f"Fichier ZIP introuvable: {fichier_zip}")

    with zipfile.ZipFile(fichier_zip, 'r') as zip_ref:
        zip_ref.extractall(repertoire_destination)
        
    pathfile='data_anonymous/data_anonymous'

    # reflist: list of epc in each box
    reflist=pd.DataFrame()
    # 
    files=os.listdir(pathfile) # retourne une liste des fichiers dans le répertoire pathfile
    for file in files:
        #print(file)
        if file.startswith('reflist_'):
            temp=pd.read_csv(os.path.join(pathfile,file),sep=',').reset_index(drop=True)[['Epc']]
            temp['refListId']=file.split('.')[0]
            reflist = pd.concat([reflist, temp])
    reflist=reflist.rename(columns={'refListId':'refListId_actual'})
    reflist['refListId_actual']=reflist['refListId_actual'].apply(lambda x:int(x[8:]))
    Q_refListId_actual=reflist.groupby('refListId_actual')['Epc'].nunique().rename('Q refListId_actual').reset_index(drop=False)
    reflist=pd.merge(reflist,Q_refListId_actual,on='refListId_actual',how='left')
    
    # 2eme partie
    
    # pathfile=r'data_anonymous'
    # 
    # df : rfid readings
    tags=pd.DataFrame()
    # 
    files=os.listdir(pathfile)
    for file in files:
        #print(file)
        if file.startswith('ano_APTags'):
            temp=pd.read_csv(os.path.join(pathfile,file),sep=',')
            tags=pd.concat([tags,temp])
    tags['LogTime']=pd.to_datetime (tags['LogTime'] ,format='%Y-%m-%d-%H:%M:%S') 
    tags['TimeStamp']=tags['TimeStamp'].astype(float)
    tags['Rssi']=tags['Rssi'].astype(float)
    tags=tags.drop(['Reader','EmitPower','Frequency'],axis=1).reset_index(drop=True)
    tags=tags[['LogTime', 'Epc', 'Rssi', 'Ant']]
    # antennas 1 and 2 are facing the box when photocell in/out 
    Ant_loc=pd.DataFrame({'Ant':[1,2,3,4],'loc':['in','in','out','out']})
    tags=pd.merge(tags,Ant_loc,on=['Ant'])
    tags=tags.sort_values('LogTime').reset_index(drop=True)
    
    # 3eme partie
    
        # timing: photocells a time window for each box: start/stop (ciuchStart, ciuchStop)
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
    
    # 4eme partie
    
    # Start_up starts upstream Start, half way in between the previous stop and the actual start
    timing[['Stop_last']]=timing[['Stop']].shift(1)
    timing[['refListId_last']]=timing[['refListId']].shift(1)
    timing['Startup']=timing['Start'] - (timing['Start'] - timing['Stop_last'])/2
    # timing start: 10sec before timing
    timing.loc[0,'refListId_last']=timing.loc[0,'refListId']
    timing.loc[0,'Startup']=timing.loc[0,'Start']-datetime.timedelta(seconds=10)
    timing.loc[0,'Stop_last']=timing.loc[0,'Startup']-datetime.timedelta(seconds=10)
    timing['refListId_last']=timing['refListId_last'].astype(int)
    # 
    timing['Stopdown']= timing['Startup'].shift(-1)
    timing.loc[len(timing)-1,'Stopdown']=timing.loc[len(timing)-1,'Stop']+datetime.timedelta(seconds=10)
    timing=timing[['refListId', 'refListId_last','Startup', 'Start','Stop','Stopdown']]
    #timing.head(30)
    
    # 5eme partie
    
    # t0_run = a new run starts when box 0 shows up
    # t0_run: c'est l'instant à partir duquel commence un run, chaque run commence avec la boxe 0
    # run : id du run
    t0_run=timing[timing['refListId']==0] [['Startup']]
    t0_run=t0_run.rename(columns={'Startup':'t0_run'})
    t0_run=t0_run.groupby('t0_run').size().cumsum().rename('run').reset_index(drop=False)
    t0_run=t0_run.sort_values('t0_run')
    # 
    # each row in timing is merged with a last row in t0_run where t0_run (ciuchstart) <= timing (ciuchstart)
    timing=pd.merge_asof(timing,t0_run,left_on='Startup',right_on='t0_run', direction='backward')
    timing=timing.sort_values('Stop')
    timing=timing[['run', 'refListId', 'refListId_last', 'Startup','Start','Stop','Stopdown','t0_run']]
    timing.head(20)
    #timing['run'].value_counts()
    
    # 6eme partie
    
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
    # 
    timing_slices=pd.merge_asof(slices,timing,left_on='slice',right_on='Startup',direction='backward')
    timing_slices=timing_slices[['run', 'refListId', 'refListId_last','slice_id','slice',  \
                             'Startup', 'Start', 'Stop', 'Stopdown','t0_run']]
    
    # 7eme partie
    
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
    # 
    timing_slices=pd.merge_asof(slices,timing,left_on='slice',right_on='Startup',direction='backward')
    timing_slices=timing_slices[['run', 'refListId', 'refListId_last','slice_id','slice',  \
                             'Startup', 'Start', 'Stop', 'Stopdown','t0_run']]
    #timing_slices.head()
    
    # 8eme partie
    
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
    df_timing_slices
    
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
    df_timing_slices_threshold=df_timing_slices[df_timing_slices['Rssi']>rssi_threshold]
    timing['window_width'] = (timing['Stopdown'] - timing['Startup']).apply(lambda x:x.total_seconds())
    timing['window_run_id'] = timing['refListId'].astype(str) +"_"+ timing['run'].astype(str)
    
    def dataset (df_timing_slices, timing, rssi_quantile ):
        # regroupe le dataset par les colonnes suivantes 'Epc', 'window_run_id', 'slice_id', 'loc' puis calule le quantile mediant c'est à dire divise le dataset en deux parties égales
        ds_rssi = df_timing_slices.groupby(['Epc', 'window_run_id', 'slice_id', 'loc'])['Rssi'].quantile(rssi_quantile)\
            .unstack(['slice_id', 'loc'], fill_value=-110)
        ds_rssi.columns = [x[0]+'_'+x[1] for x in ds_rssi.columns]
        ds_rssi = ds_rssi.reset_index(drop = False)

        ds_rc = df_timing_slices.groupby(['Epc', 'window_run_id', 'slice_id', 'loc']).size()\
            .unstack(['slice_id', 'loc'], fill_value=0)
        ds_rc.columns = [x[0]+'_'+x[1] for x in ds_rc.columns]
        ds_rc = ds_rc.reset_index(drop = False)

        ds = pd.merge(ds_rssi, ds_rc, on=['Epc', 'window_run_id'], suffixes=['_rssi', '_rc'])

        #window_width

        ds = pd.merge(ds, timing[['window_run_id', 'window_width']], on ='window_run_id', how='left')

            #Epcs_window

        Q_Epcs_window = df_timing_slices.groupby(['window_run_id'])['Epc'].nunique().rename('Epcs_window').reset_index(drop=False)
        ds= pd.merge(ds, Q_Epcs_window, on='window_run_id', how='left')

        #reads_window

        Q_reads_window = df_timing_slices.groupby(['window_run_id']).size().rename('reads_window').reset_index(drop=False)
        ds= pd.merge(ds, Q_reads_window, on= 'window_run_id', how='left')
        ds = pd.merge(ds, reflist, on='Epc', how='left')

        ds['actual']= ds['window_run_id'].apply(lambda x:x.split('_')[0]).astype('int64') == ds['refListId_actual']

        ds['actual'] = ds.apply(lambda row: 'in' if int(row['window_run_id'].split('_')[0]) == row['refListId_actual'] else 'out', axis=1)
        vrai=ds[ds['actual']== 'in']
        faux=ds[ds['actual']== 'out']
        true= vrai.shape[0]
        #print('true',true)
        false= faux.shape[0]
        #print('false',false)

        #print(ds.groupby('actual')['Epc'].nunique())


        return ds
    a =  dataset (df_timing_slices, timing, 1)
    

    Features=pd.DataFrame(
    [
        ['all', True, True, False, True, True, True ],
        ['rssi & rc only', True, True, False, False, False, False],
        ['rssi & rc_mid', True, True, True, False, False, False ],
        ['rssi only', True, False, True, False, False, False ],
        ['rc only', False, True, False, False, False, False ],
    ],
        columns=['features', 'rssi', 'rc', 'rc_mid_only', 'Epcs_window', 'reads_window', 'window_width']
    )
    #Features
    
    def Xcols_func(features, Xcols_all):
    
        Features_temp = Features [Features['features']==features]

        X=[]
        rssi = Features_temp ['rssi'].values[0]
        rc = Features_temp['rc'].values[0]
        rc_mid_only = Features_temp['rc_mid_only'].values[0]
        Epcs_window = Features_temp['Epcs_window'].values[0]
        reads_window = Features_temp['reads_window'].values[0]
        window_width = Features_temp['window_width'].values[0]

        X_rssi = [x for x in Xcols_all if rssi*'rssi' in x.split('_') ]

        X_rc = [x for x in Xcols_all if rc*'rc' in x.split('_') ]

        X = X_rssi + X_rc

        if Epcs_window:
            X.append('Epcs_window')
        if Epcs_window:
            X.append('reads_window')
        if Epcs_window:
            X.append('window_width')

        return X


    b=Xcols_func('all', a.columns)  
    
    def filter_columns(df, column_names):
        filtered_df = pd.DataFrame()  # Créer un nouveau DataFrame pour stocker les résultats filtrés
        for column in column_names:
            if column in df.columns:
                # Si le nom de la colonne est présent dans le DataFrame d'origine
                # Ajouter la colonne correspondante dans le DataFrame filtré
                filtered_df[column] = df[column]
        return filtered_df

    X=filter_columns(a, b)
    
    

    # Créer un classificateur Random Forest

    #n_estimators_rm = 100   #n_estimators nombre d'abre de decision
    #max_depth_rm = 10 #max_depth La profondeur de chaque arbre de decision
    #min_samples_split_rm = 2#min_samples_plit Le nombre minimum d'échantillons requis pour diviser un noeud interne
    #min_samples_leaf_rm = 1 #min_samples_leaf Le nombre minimum d'échantillons requis pour etre un noeud
    #bootstrap_rm = True#bootstrap Indique si les échantillons sont tirés avec remplacement lors de la construction des arbres

    #un nœud fait référence à un point de division dans un arbre de décision. Lorsque vous construisez un arbre de décision 



    clf = RandomForestClassifier(n_estimators = n_estimators_rm, max_depth = max_depth_rm, min_samples_split = min_samples_split_rm, min_samples_leaf = min_samples_leaf_rm, bootstrap=bootstrap_rm)

    pred_ml=pd.DataFrame()
    retries = 2
    y = a['actual']

    # Liste pour stocker les précisions de chaque tour
    accuracies = []

    for retry in range(retries):

        Xtrain, Xtest, ytrain, ytest = train_test_split(X, y, train_size=0.8, stratify=y)

        scaler = MinMaxScaler()
        scaler.fit(Xtrain)
        Xtrain_std = scaler.transform(Xtrain)
        Xtest_std = scaler.transform(Xtest)

        clf.fit(Xtrain_std, ytrain)

        ypred = clf.predict(Xtest_std)
        scores = cross_val_score(clf, Xtrain_std, ytrain, cv=5)

        accuracy = (ytest == ypred).mean()

        # Imprimer l'accuracy de chaque tour
        #print(retry, accuracy, scores, scores.mean())

        # Ajouter l'accuracy à la liste
        accuracies.append(accuracy)

        #print(retry, accuracy,scores,scores.mean()) 
        ypred_series = pd.Series(ypred, index=ytest.index, name='pred_ml')
        temp = a.loc[ytest.index, ['Epc', 'window_run_id', 'actual']]

        temp = temp.join(ypred_series)
        temp.loc[:, 'retry'] = retry
        pred_ml = pd.concat([pred_ml, temp], ignore_index=True)


    pred_ml.loc[:, 'pred_ml_bool'] = (pred_ml.loc[:, 'actual'] == pred_ml.loc[:, 'pred_ml'])
    pred_ml = pred_ml [['Epc', 'window_run_id', 'actual', 'pred_ml', 'pred_ml_bool', 'retry']]

    # Calculer la moyenne des précisions
    mean_accuracy = sum(accuracies) / len(accuracies)
    #print("Moyenne des précisions:", mean_accuracy)

    #pred_ml
    #pred_ml.to_excel('pred_ml.xlsx', sheet_name='True Counts')  
    return mean_accuracy


#random_forest=random_forest(100,10,2,1,True)
#random_forest


# In[ ]:




