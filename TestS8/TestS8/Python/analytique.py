import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import datetime
import tarfile
from sklearn.metrics import confusion_matrix, accuracy_score
from config import dataframe


def analytical():
    df_timing_slices = dataframe['df_timing_slices']
    timing_slices = dataframe['timing_slices']
    reflist = dataframe['reflist']
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





