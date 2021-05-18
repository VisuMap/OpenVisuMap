# ShowGraph.py
#============================================================================================
import tensorflow as tf
import sys
from ModelUtil import *

def dump(obj):
   for attr in dir(obj):
       if hasattr( obj, attr ):
           print( "obj.%s = %s" % (attr, getattr(obj, attr)))

md = ModelBuilder()
md.LoadModel(sys.argv[1])
g = tf.get_default_graph().as_graph_def()
tf.graph_util.remove_training_nodes(g)
md.ShowGraph()
