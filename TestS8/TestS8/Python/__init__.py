import config
import test1
from pretraitement import pretraitement
from analytique import analytical
from randomforest import random_forest

pretraitement()
#print(analytical())
print(random_forest(100,10,2,1,True))
