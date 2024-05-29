import config
import test1
from pretraitement import pretraitement
from analytique import analytical
from randomforest import random_forest
from knn import knn_model

#pretraitement()
#knn_model(5,'uniform','auto',2)
print(random_forest(100,10,2,1,True))

