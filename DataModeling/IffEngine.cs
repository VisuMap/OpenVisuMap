using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisuMap.DataModeling {
    // Feedforward Network Engine
    public interface IffEngine {
        bool SetTrainingData(double[][] inputData, double[][] outputData);

        bool SetValidationData(double[][] validationData);

        bool StartTraining();

        void StopTraining();

        void WaitForCompletion();

        double[][] Evaluate(double[][] testData);

        double[][] ReadOutput();  // read intermediated predication output.

        DataLink Link { get; set; }

        string Name { get; set; }

        int MaxEpochs { get; set; }

        int LogLevel { get; set; }

        int RefreshFreq { get; set; }
    }
}
