import tensorflow as tf
import numpy as np
import sys,os

modelName = sys.argv[1]
sess = tf.Session()
saver = tf.train.import_meta_graph(modelName + '.meta')
saver.restore(sess, tf.train.latest_checkpoint('./', modelName + '.chk'))

saved_vars = tf.get_collection('vars');
var_x = saved_vars[0]
y = saved_vars[1]
keep_prob=saved_vars[2]
h = saved_vars[3]

test_data = np.genfromtxt('inData.csv', delimiter='|', dtype=np.float32)
output = sess.run(h, feed_dict={var_x:test_data, keep_prob:1.0})
N = np.shape(test_data)[0]
np.savetxt("outData.csv", output, delimiter='|', fmt='%.5f')
