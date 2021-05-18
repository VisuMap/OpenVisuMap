Data Modelling Plugin for VisuMap
=================================

Overview
--------

Data Modeling Plugin (DMP) is a plugin for VisuMap to create artificial
neural network (ANN) models to perform data classification and mapping services.
Those ANN models are trained with information from colored (or labeled) maps created
in VisuMap by various unsupervised classification and mapping algorithms.
Trained ANN models can then be applied to new similar data to create colored maps for
comparison with reference maps.

Additionally, DMP offers the service to create 2D or 3D maps from high dimensional
data by means of autoencoder ANN. 

DMP currently uses the TensorFlor from Google Inc. as machine learning engine.
DMP provides GUI interfaces to create, apply and manage TensorFlow models.

Installation
------------
DMP 1.0 can be directly installed from VisuMap's plugin website through it's main
menu Help->Check For Plugin->Data Modeling. Before using DMP, the following 
depended software packages need to be installed on the desktop machine: 
 
   1. Python 3.5 64 bit version for Windows.

   2. Google TensorFlow 1.0.1. See the following page for more details:

          https://www.tensorflow.org/install/install_windows

   3. Optionally, if GPU acceleration is wanted, NVIDA CUDA 8.0 and the GPU 
      version of TensorFlow. In this case, the GPU version can be installed by the
      the following command:

          pip3 install --upgrade tensorflow-gpu


Deep profiling with neural networks
-----------------------------------

The main service of DMP plugin is to create ANN models to learn reference maps of high 
dimensional dataset. The trained models can then be applied to new data to create new maps
which then serve as a kind of profile for the new data.

A normal use case for deep profiling with DMP is as follows:

1. Loaded the sample dataset file ModelingSamples.xvmz into VisuMap. This dataset file contains two
   tables named Training Data and Test Data. The first will be used for training; the second for
   testing.

2. Open the Training Data table and the map named Initial; then create a MDS map with a 
   mapping algorithm of VisuMap. For this test, we recommend using the t-SNE algorithm 
   to create a 2D map.

3. Use a clustering algorithm in VisuMap to argument clusters in the map with different colors
   and glyphs. We recommend using the k-Mean algorithm to perform this task. After
   the clustering task, we recommand some manually adjustments of the clusters directly on the 
   map through the GUI interface of VisuMap.

4. Open the Modeling Training panel through the menu Plugins->Model Training. In the training panel
   set the Model Name field to "Mapping Model"; set the learning target to "shape"; set the Max Epochs
   to 1000. Then click on the "run" arrow to start the training process which may last few minutes.

5. In the Modeling training panel opened in previous step, change the model name to "Classification Model"; 
   change the learning target to "Coloring".  Then click on the "run" arrow to start the training process.
   Close the training panel after the training has completed.

6. The previous two steps created two ANN models named "Mapping Model" and "Classification Model". Now we
   apply these models to new data. To do so, we open the test data table "Test Data". Then open model
   test panel through the menu Plugins->Model Evaluation. The model name drop-down list will list all models
   valid models in the current environment for the currently loaded dataset. Let's select first the model 
   "Mapping Model" and click on the "Apply Model" button; then select "Classification Model" and
   click on the "Apply Model". This two tasks will create a colored map for the new dataset. 

7. Click on the menu View->Map Snapshot to create a snapshot of the test data; Then switch back to the
   training dataset and click on the menu ->View Map Snapshot again to open a snapshot for the 
   training data.  Comparing these two snapshots we can then easily see the differences between their 
   clusters structures

Model Manager
-------------

The model manager is opened through the menu Plugins->Model Manager. When opened, the manager will
list all awailable trained models in current environment (which is the home directory of the plugin.)
The model manager enables you to do the following tasks:

- Delete the selected models.

- Import Models. You will be prompt for a zip file name that contains training models. 

- Export Models. The selected models will packaged into a zip file.

- Rename Model. You will be prompt for a new name for the first selected model.


Script interface
----------------

DMP provides script interface to access its major services. The follows sample script file
TrainModel.js demonstrates how to use the script APIs:

// File: TrainModel.js
//============================================
var ms = vv.FindPluginObject("DMScript");

// Train a model.
var trainer = ms.NewTrainer();
var mdName = "TestModel";
trainer.ReadOnly = true;
trainer.LogLevel = 1;
trainer.MaxEpochs = 300;
trainer.LearningTarget = 1;
trainer.ModelName = mdName;
trainer.Show();
trainer.StartTraining();
trainer.Close();

// Apply the trained model.
var tester = ms.NewTester();
tester.Show();
tester.SelectModel(mdName);
tester.DoPrediction();
tester.Close();
vv.Sleep(1000);
vv.GuiManager.RecoverLastMap();
