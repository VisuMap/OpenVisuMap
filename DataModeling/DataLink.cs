using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using VisuMap.Plugin;
using VisuMap.Script;
using IDataset = VisuMap.Script.IDataset;

namespace VisuMap.DataModeling {
    public class DataLink {
        public const string VarColor = "<Coloring>";
        public const string Var3DPos = "<XYZ>";
        public const string Var2DPos = "<XY>";
        public const string Var2DPosClr = "<XYColor>";
        public const string Var3DPosClr = "<XYZColor>";
        public const string FilterTarget = "<Filter>";
        public const string ColumnList = "<ColList>";
        public const string VarClsType = "<Cls>";
        public const string Nul = "<Nul>";
        string nmFmt = "g7";

        public enum ScalingMethods {
            None = 0, GlobalRange, VariableRange
        }

        Dictionary<short, short> type2Index;
        double mapFactor = 1.0;
        public const double MapMargin = 0.15;
        int mapWidth;
        int mapHeight;
        int mapDepth;

        string trDatasetName;
        string trMapName;
        string modelType;
        string modelName;
        int trSamples;
        DateTime lastUpdate;
        int trEpochs;

        string inputGroupName = "";
        IList<string> inputVariables;
        IList<double> inputFactors;
        IList<double> inputShifts;

        string traningTarget;  // The training target type of: Shp, Clr, ShpClr, etc.
        string outputLabel;    // Expanded from trainingTarget: <XY>, <XYZ>, <XYColor>
        string outputGroupName = ""; // otpional group name in "Var:<Group>" training target.
        IList<string> outputVariables = null;
        IList<double> outputFactors = null;
        IList<double> outputShifts = null;

        ScalingMethods inputScaling = ScalingMethods.GlobalRange;
        ScalingMethods outputScaling = ScalingMethods.GlobalRange;

        public DataLink() {
        }

        public DataLink(string filePath) {
            LoadModelInfo(filePath);
        }

        public DateTime LastUpdate {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        public string ModelType {
            get { return modelType; }
            set { modelType = value; }
        }

        public int TrainingEpochs  {
            get { return trEpochs; }
            set { trEpochs = value; }
        }

        
        public string OutputLabel {
            get { return outputLabel; }
            set { outputLabel = value; }
        }

        public int MapWidth {
            get { return mapWidth; }
            set { mapWidth = value; }
        }

        public int MapHeight {
            get { return mapHeight; }
            set { mapHeight = value; }
        }

        public int MapDepth {
            get { return mapDepth; }
            set { mapDepth = value; }
        }

        public string ModelName {
            get { return modelName; }
            set { modelName = value; }
        }

        public string TrainingDatasetName {
            get { return trDatasetName; }
            set { trDatasetName = value; }
        }

        public string TrainingMapName {
            get { return trMapName; }
            set { trMapName = value; }
        }

        public int TrainingSamples
        {
            get { return trSamples; }
            set { trSamples = value; }
        }

        public Dictionary<short, short> Type2Index {
            get { return type2Index; }
            set { type2Index = value; }
        }

        public bool IsClassifier {
            get { return outputLabel.Equals(VarColor); }
        }


        public int MapDimension
        {
            get
            {
                if ((outputLabel == DataLink.Var3DPos) || (outputLabel == DataLink.Var3DPosClr)) {
                    return 3;
                } else if ((outputLabel == DataLink.Var2DPos) || (outputLabel == DataLink.Var2DPosClr)) {
                    return 2;
                } else {
                    return DataModeling.App.ScriptApp.Map.Dimension;
                }
            }
        }

        public bool HasClassInfo {
            get { return (outputLabel == Var2DPosClr) || (outputLabel == Var3DPosClr) || (outputLabel == VarColor); }
        }

        public bool HasPosInfo {
            get { return (outputLabel==null)?false: outputLabel.StartsWith("<XY"); }
        }

        public string TraningTarget { get => traningTarget; set => traningTarget = value; }
        public IList<string> OutputVariables { get => outputVariables; set => outputVariables = value; }
        public IList<double> OutputFactors { get => outputFactors; set => outputFactors = value; }
        public IList<double> OutputShifts { get => outputShifts; set => outputShifts = value; }

        public IList<string> InputVariables { get => inputVariables; set => inputVariables = value; }
        public IList<double> InputFactors {  get => inputFactors;  set => inputFactors = value; }
        public IList<double> InputShifts { get => inputShifts; set => inputShifts = value; }
        public double MapFactor  {  get=>mapFactor; set=>mapFactor = value; }

        public string InputGroupName { get => inputGroupName; }


        public ScalingMethods InputScaling { get => inputScaling; set => inputScaling = value; }
        public ScalingMethods OutputScaling { get => outputScaling; set => outputScaling = value; }
      
        INumberTable CreateColorTable() {
            return CreateColorTable(DataModeling.App.ScriptApp.Dataset.BodyListEnabled());
        }

        INumberTable CreateColorTable(IList<IBody> bodyList) {
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;

            type2Index = new Dictionary<short, short>();
            for (int row = 0; row < bodyList.Count; row++) {
                short t = bodyList[row].Type;
                if (!type2Index.ContainsKey(t))
                    type2Index.Add(t, (short)type2Index.Count);
            }
            int N = type2Index.Count;
            INumberTable nt = app.New.NumberTable(bodyList.Count, N);
            for (int row = 0; row < bodyList.Count; row++) {
                nt.Matrix[row] = new double[N];
                short t = type2Index[bodyList[row].Type];
                if (t < N) {
                    nt.Matrix[row][t] = 1.0;
                }
                nt.RowSpecList[row].Id = bodyList[row].Id;
                nt.RowSpecList[row].Name = bodyList[row].Name;
                nt.RowSpecList[row].Type = bodyList[row].Type;
            }
            return nt;
        }

        INumberTable CreateClassTypeTable() {
            var bodyList = DataModeling.App.ScriptApp.Dataset.BodyListEnabled();
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;
            INumberTable nt = app.New.NumberTable(bodyList.Count, 1);
            for (int row = 0; row < bodyList.Count; row++) {
                nt.Matrix[row][0] = (double)bodyList[row].Type;
                nt.RowSpecList[row].Id = bodyList[row].Id;
                nt.RowSpecList[row].Name = bodyList[row].Name;
                nt.RowSpecList[row].Type = bodyList[row].Type;
            }
            return nt;
        }

        INumberTable CreateXYZTable(string targetName, int dimension) {
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;

            var bodyList = dataset.BodyListEnabled();
            var nt = app.New.NumberTable(bodyList.Count, dimension);

            mapFactor = CalculateMapFactor(app.Map, dimension);

            for (int row = 0; row < bodyList.Count; row++) {
                var b = bodyList[row];
                nt.Matrix[row] = (dimension == 3) ? new double[3] { b.X, b.Y, b.Z } : new double[2] { b.X, b.Y };
                for (int col = 0; col < dimension; col++) {
                    nt.Matrix[row][col] = nt.Matrix[row][col] * mapFactor + MapMargin;
                }
                nt.RowSpecList[row].Id = bodyList[row].Id;
                nt.RowSpecList[row].Name = bodyList[row].Name;
                nt.RowSpecList[row].Type = bodyList[row].Type;
            }
            nt.ColumnSpecList[0].Id = "X";
            nt.ColumnSpecList[1].Id = "Y";
            if (dimension == 3)
                nt.ColumnSpecList[2].Id = "Z";

            return nt;
        }

        public static IList<string> ToColumnList(string groupName, INumberTable nt) {
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;
            List<string> cIds = null;

            var filter = app.Folder.OpenTableFilter(groupName);
            if (filter != null) {
                cIds = dataset.ColumnSpecList.Where((cs, col) => (cs.IsNumber && filter.Enabled[col])).Select(cs => cs.Id).ToList();
            } else {
                cIds = new List<string>();
                var idList = app.GroupManager.GetGroupLabels(groupName);
                if (idList != null) {
                    foreach (var id in idList) {
                        if (nt.IndexOfColumn(id) >= 0)
                            cIds.Add(id);
                    }
                } else {
                    // try to interpret filterName as a list of column ids separated by '|'
                    foreach (string cId in groupName.Split('|')) {
                        if (nt.IndexOfColumn(cId) >= 0)
                            cIds.Add(cId);
                    }
                }
            }
            return cIds;
        }

        INumberTable CreateTableByFilter(string filterName, bool forInput) {
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;
            var nt = dataset.GetNumberTableEnabled();
            nt.CheckForWrite();

            if (forInput) {
                inputFactors = Enumerable.Repeat(1.0, nt.Columns).ToList();
            } else {
                outputFactors = Enumerable.Repeat(1.0, nt.Columns).ToList();
            }

            IList<string> cIds = null;

            if (string.IsNullOrEmpty(filterName) || (filterName == "*")) {
                cIds = nt.ColumnSpecList.Select(cs => cs.Id).ToList();
            } else {
                cIds = ToColumnList(filterName, nt);
                if (cIds.Count == 0) {
                    MessageBox.Show(filterName + " is not the name of filter or variable group!");
                    return nt;
                }
                nt = nt.SelectColumnsById(cIds);
            }

            List<double> factors = Enumerable.Repeat(1.0, nt.Columns).ToList();
            List<double> shifts = Enumerable.Repeat(0.0, nt.Columns).ToList();

            var scaling = forInput ? inputScaling : outputScaling;
            if ( scaling != ScalingMethods.None ) {
                var colMax = new double[nt.Columns];
                var colMin = new double[nt.Columns];
                var colRange = new double[nt.Columns];

                for (int col = 0; col < nt.Columns; col++) {
                    colMax[col] = double.MinValue;
                    colMin[col] = double.MaxValue;
                }

                for (int row=0; row<nt.Rows; row++) {
                    var R = nt.Matrix[row];
                    for (int col = 0; col < nt.Columns; col++) {
                        colMax[col] = Math.Max(R[col], colMax[col]);
                        colMin[col] = Math.Min(R[col], colMin[col]);
                    }
                }
                for (int col = 0; col < nt.Columns; col++) 
                    colRange[col] = Math.Max(0.000001, colMax[col] - colMin[col]);

                // linear scaling and shifting to the range [0, 1.0]
                double gape = forInput ? 0.0 : 0.025;
                if ( scaling == ScalingMethods.VariableRange ) {
                    for (int col = 0; col < nt.Columns; col++) {
                        double gapeSize = gape * colRange[col];
                        shifts[col] = colMin[col] - gapeSize;
                        factors[col] = 1.0 / (colRange[col] + 2*gapeSize);
                    }
                } else if (scaling == ScalingMethods.GlobalRange) {
                    double vRange = colMax.Max() - colMin.Min();
                    double gapeSize = gape*vRange;
                    double vFactor = 1/(vRange + 2*gapeSize);
                    double vShift = colMin.Min() - gapeSize;
                    for (int col = 0; col < nt.Columns; col++) {
                        shifts[col] = vShift;
                        factors[col] = vFactor;
                    }
                }

                for (int row = 0; row < nt.Rows; row++) {
                    var R = nt.Matrix[row];
                    for (int col = 0; col < nt.Columns; col++) {
                        R[col] = (R[col] - shifts[col]) * factors[col];
                    }
                }
            }

            if (forInput) {
                inputFactors = factors;
                inputShifts = shifts;
                inputVariables = cIds;
            } else {
                outputFactors = factors;
                outputShifts = shifts;
                outputVariables = cIds;
            }
            return nt;
        }

        INumberTable GetNumberTableByColumns(IList<string> columnList, IList<double> factors, IList<double> shifts) {
            var nt = DataModeling.App.ScriptApp.GetNumberTable().SelectColumnsById(columnList);
            foreach(var R in nt.Matrix){
                for (int col = 0; col < nt.Columns; col++)
                    R[col] = (R[col] - shifts[col]) * factors[col];
            }
            return nt;
        }

        public INumberTable GetNumberTable(string filterName, bool inputTable) {
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;
            INumberTable nt = null;

            switch( filterName ) {
                case DataLink.VarColor:
                    nt = CreateColorTable();
                    break;

                case DataLink.Var3DPos:
                case DataLink.Var2DPos:
                case DataLink.Var2DPosClr:
                case DataLink.Var3DPosClr:

                    int dimension = ((filterName == DataLink.Var3DPos) || (filterName == DataLink.Var3DPosClr)) ? 3 : 2;
                    nt = CreateXYZTable(filterName, dimension);
                    if ((filterName == DataLink.Var2DPosClr) || (filterName == DataLink.Var3DPosClr)) {
                        nt.AppendColumns(CreateColorTable());
                    }
                    break;

                case DataLink.ColumnList:
                    break;

                case DataLink.VarClsType:
                    nt = CreateClassTypeTable();
                    break;

               default:  // filterName is an actual filter name.
                    nt = CreateTableByFilter(filterName, inputTable);
                    break;
            }
            return nt;
        }

        public static int GetTargetDimension(string filterName) {
            int columns = 0;
            var app = DataModeling.App.ScriptApp;
            int clrs = app.Dataset.BodyListEnabled().Select(b => b.Type).Distinct().Count();
            switch ( filterName ) {
                case DataLink.VarColor:
                    columns = clrs;
                    break;

                case DataLink.Var3DPos:
                    columns = 3;
                    break;
                case DataLink.Var2DPos:
                    columns = 2;
                    break;

                case DataLink.Var2DPosClr:
                    columns = 2 + clrs;
                    break;

                case DataLink.Var3DPosClr:
                    columns = 3 + clrs;
                    break;

                default:  // filterName is an actual filter name.
                    columns = GetFilterGroupDimension(filterName);
                    break;
            }

            return columns;
        }
        // returns the dimension of group or filter.
        static int GetFilterGroupDimension(string filterName) {
            var app = DataModeling.App.ScriptApp;
            if (string.IsNullOrEmpty(filterName) || (filterName=="*")) {
                return app.Dataset.ColumnSpecList.Count(cs => cs.IsNumber);
            } else {
                var filter = app.Folder.OpenTableFilter(filterName);
                if ( filter == null ) { // assume it is group
                    var g = app.GroupManager.GetGroupLabels(filterName);
                    if (g != null)
                        return g.Count;
                    else
                        return filterName.Split('|').Length;  // interpret the filter name as a list of column ids.
                } else if (filter.Columns == app.Dataset.Columns) {
                    return app.Dataset.ColumnSpecList.Where((cs, col) => (cs.IsNumber && filter.Enabled[col])).Count();
                } 
            }
            return 0;
        }

        public short[] GetIndex2Type() {
            if (type2Index == null) return null;
            short[] index2types = new short[type2Index.Count];
            foreach (var t in type2Index.Keys) index2types[type2Index[t]] = t;
            return index2types;
        }

        public Tuple<double[][], double[][], double[][]> CreateTrainingDataNew(FeedforwardNetwork learner, string validationData) {
            INumberTable ntInput = null;
            if (inputGroupName != DataLink.Nul) {
                ntInput = GetNumberTable(inputGroupName, true);
                if ((ntInput.Rows * ntInput.Columns) == 0) {
                    throw new DataModelingException("No input training data configured.");
                }
            }

            INumberTable ntOutput = null;
            string outDataSpec = TargetDescription();
            if (outDataSpec != DataLink.Nul) {
                ntOutput = (outDataSpec == DataLink.ColumnList)
                    ? GetNumberTableByColumns(outputVariables, outputFactors, outputShifts) : GetNumberTable(outDataSpec, false);

                if (ntOutput.Rows == 0) {
                    throw new DataModelingException("No output training data configured.");
                }

                if ((outDataSpec == DataLink.VarColor) && (ntOutput.Columns <= 1)) {
                    throw new DataModelingException("No output classification data available.");
                }
            }

            if (ntOutput != null) {
                string header = "";
                if (this.OutputLabel == DataLink.ColumnList) {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Factors:");
                    foreach (var fct in this.OutputFactors)
                        sb.Append(' ').Append(fct);
                    header = sb.ToString();
                } else if (this.OutputLabel.StartsWith("<")) {
                    header = "MapFactor: " + this.MapFactor.ToString();
                } else {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Factors:");
                    foreach (var f in this.OutputFactors) {
                        sb.Append(' ');
                        sb.Append(f.ToString("g7"));
                    }
                    header = sb.ToString();
                }
            }

            var app = DataModeling.App.ScriptApp;
            trDatasetName = app.Dataset.Name;
            trMapName = app.Map.Name;
            trSamples = (ntInput == null) ? app.Dataset.Rows : ntInput.Rows;
            mapWidth = (int)app.Map.Width;
            mapHeight = (int)app.Map.Height;
            mapDepth = (int)app.Map.Depth;

            outputLabel = outDataSpec;

            var valData = GetValidationData(validationData);

            return new Tuple<double[][], double[][], double[][]>(ntInput?.Matrix as double[][], ntOutput?.Matrix as double[][], valData);
        }

        double CalculateMapFactor(IMapLayout layout, int dimension) {
            double factor = Math.Max(layout.Width, layout.Height);
            if (dimension == 3) {
                factor = Math.Max(factor, layout.Depth);
            }
            //factor *= dimension;
            return (1.0 - 2 * MapMargin) / factor;
        }

        INumberTable GetOutputData(string target, IList<IBody> bodyList, IMapLayout mapLayout) {
            var app = DataModeling.App.ScriptApp;
            if (target == DataLink.VarColor) {
                return CreateColorTable(bodyList);
            } else {
                int dimension = ((target == DataLink.Var3DPos) || (target == DataLink.Var3DPosClr)) ? 3 : 2;
                var nt = app.New.NumberTable(bodyList.Count, dimension);

                mapFactor = CalculateMapFactor(mapLayout, dimension);

                for (int row = 0; row < bodyList.Count; row++) {
                    var b = bodyList[row];
                    nt.Matrix[row] = (dimension == 3) ? new double[3] { b.X, b.Y, b.Z } : new double[2] { b.X, b.Y };
                    for (int col = 0; col < dimension; col++) 
                        nt.Matrix[row][col] = nt.Matrix[row][col] * mapFactor + MapMargin;
                    nt.RowSpecList[row].Id = bodyList[row].Id;
                    nt.RowSpecList[row].Name = bodyList[row].Name;
                    nt.RowSpecList[row].Type = bodyList[row].Type;
                }
                nt.ColumnSpecList[0].Id = "X";
                nt.ColumnSpecList[1].Id = "Y";
                if (dimension == 3)
                    nt.ColumnSpecList[2].Id = "Z";

                if ((target == DataLink.Var2DPosClr) || (target == DataLink.Var3DPosClr)) {
                    nt.AppendColumns(CreateColorTable(bodyList));
                }

                return nt;
            }
        }

        IList<IRowSpec> inputRowSpec = null;

        double[][] ScaleShiftMatrix(double[][] matrix) {
            int columns = matrix[0].Length;
            if (inputFactors != null) {
                foreach (var Row in matrix)
                    for (int col = 0; col < columns; col++)
                        Row[col] = (Row[col] - inputShifts[col]) * inputFactors[col];
            }
            return matrix;
        }

        public double[][] GetTestInputData(bool justSelected, bool allowPartialData) {
            var app = DataModeling.App.ScriptApp;            
            var nt = justSelected ? app.GetSelectedNumberTable() : app.Dataset.GetNumberTableEnabled();
            if ( justSelected && (nt.Rows == 0) )
                nt = app.Dataset.GetNumberTableEnabled();
            nt = allowPartialData ? nt.SelectColumnsById2(inputVariables, 0) : nt.SelectColumnsById(inputVariables);
            nt.CheckForWrite();
            if (inputFactors != null) {
                foreach (var Row in nt.Matrix)
                    for (int col = 0; col < inputVariables.Count; col++)
                        Row[col] = (Row[col] - inputShifts[col]) * inputFactors[col];
            }
            inputRowSpec = nt.RowSpecList;

            return nt.Matrix as double[][];
        }

        public double[][] GetPartialInputData(bool justSelected = false) {
            var app = DataModeling.App.ScriptApp;
            var nt = justSelected ? app.GetSelectedNumberTable() : app.Dataset.GetNumberTableEnabled();
            if (justSelected && (nt.Rows == 0))
                nt = app.Dataset.GetNumberTableEnabled();

            var varToIdx = new Dictionary<string, int>();
            var csList = nt.ColumnSpecList;
            for (int i = 0; i < csList.Count; i++)
                varToIdx[csList[i].Id] = i;
            int columns = inputVariables.Count;
            int[] idx2idx = new int[columns];
            for(int i=0; i< columns; i++) {
                string nm = inputVariables[i];
                idx2idx[i] = varToIdx.ContainsKey(nm) ? varToIdx[nm] : -1;
            }

            double[][] m = nt.Matrix as double[][];
            double[][] matrix = new double[nt.Rows][];
            for (int row = 0; row < nt.Rows; row++) {
                double[] R = matrix[row] = new double[columns];
                for(int col=0; col< columns; col++) {
                    int cIdx = idx2idx[col];
                    R[col] = (cIdx >= 0) ? m[row][cIdx] : 0;
                }
            }

            inputRowSpec = nt.RowSpecList;
            return ScaleShiftMatrix(matrix);
        }


        public IList<IBody> ValidationBodies = null;
        public double[][] ValidationOutput = null;  // only set for variable output.

        public double[][] GetValidationData(string dataSource) {
            if (string.IsNullOrEmpty(dataSource)) {
                ValidationBodies = null;
                return null;
            }

            var app = DataModeling.App.ScriptApp;
            string[] fs = dataSource.Split(':');
            string dsName = null;
            string mapName = null;
            if (fs.Length == 2) {
                dsName = fs[0];
                mapName = fs[1];
            } else if (fs.Length == 1) {
                dsName = app.Dataset.Name;
                mapName = fs[0];
            } else {
                throw new Exception("Invalidate data source string!");
            }

            var ds = (dsName== app.Dataset.Name) ? app.Dataset : app.Folder.ReadDataset(dsName);
            if (ds == null)
                throw new Exception("Cannot load dataset: " + dsName);

            var bodies = ds.BodyList.Select(b => b.Clone()).ToList();
            var map = ds.ReadMap2(mapName, bodies);
            if (map == null)
                throw new Exception("Cannot load map: " + mapName + "; " + app.LastError);

            ValidationBodies = bodies.Where(b=>!b.Disabled).ToList();
            var nt = ds.GetNumberTable2(ValidationBodies.Select(b => b.Id).ToList());

            if ((traningTarget == DataLink.tt.Var) || (traningTarget == DataLink.tt.Mdl)) {
                ValidationOutput = nt.SelectColumnsById(outputVariables).Matrix as double[][];
            }

            nt = nt.SelectColumnsById(inputVariables);
            double[][] matrix = nt.Matrix as double[][];
            if (inputFactors != null) {
                foreach (var Row in matrix)
                    for (int col = 0; col < nt.Columns; col++)
                        Row[col] = (Row[col] - inputShifts[col]) * inputFactors[col];
            }
            return matrix;
        }

        public Tuple<int, double> LinkResult(double[][] output) {
            Tuple<int, double> ret = null;

            if ( (output == null) || (output.Length==0) || (output[0].Length == 0) ) {
                MessageBox.Show("No result data available.");
                return ret;
            }

            var app = DataModeling.App.ScriptApp;
            if (outputLabel.StartsWith("<") && (outputLabel != DataLink.Nul) ) {
                VisuMap.Script.IDataset dataset = app.Dataset;
                app.GuiManager.RememberCurrentMap();

                ret = LinkResult(output, dataset.BodyListEnabled(), this, DataModeling.App.ScriptApp.Map);
                app.Folder.DataChanged = true;
                app.Map.DataChanged = true;
                app.Map.RedrawAll();
            } else {
                if ((outputFactors != null) && (outputShifts != null)) {
                    for (int row = 0; row < output.Length; row++) {
                        for (int col = 0; col < output[0].Length; col++)
                            output[row][col] = output[row][col] / outputFactors[col] + outputShifts[col];
                    }
                }

                var nt = app.New.NumberTable(output);
                if ( (inputRowSpec!=null) && (inputRowSpec.Count ==nt.Rows)) {
                    for (int row = 0; row < nt.Rows; row++)
                        nt.RowSpecList[row].CopyFrom(inputRowSpec[row]);
                }

                var csList = nt.ColumnSpecList;
                var csList2 = app.Dataset.ColumnSpecList;
                var vNames = OutputLabel.Split(' ');
                for(int col=0; col<vNames.Length; col++) {
                    var col2 = nt.IndexOfColumn(vNames[col]);
                    if (col2 >= 0)
                        csList[col].CopyFrom(csList2[col2]);
                }
                app.New.HeatMap(nt).Show();
            }

            return ret;
        }

        /// <summary>
        /// Find the index with the maximal value.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int MaxIndex(double[] v, int index) {
            int maxIdx = -1;
            double maxValue = double.MinValue;
            for (int i = index; i < v.Length; i++) {
                if ( v[i] >= maxValue ) {
                    maxIdx = i;
                    maxValue = v[i];
                }
            }
            return maxIdx;
        }

        public static Tuple<int, double> LinkResult(double[][] output, IList<IBody> bodyList, DataLink lnk, IMap map=null) {
            if ((output == null) || (output.Length == 0) || (output[0].Length == 0)) {
                MessageBox.Show("No result data available.");
                return null;
            }
            int mismatches = 0;
            double L1Error = 0;

            switch (lnk.OutputLabel) {
                case DataLink.Var2DPos:
                case DataLink.Var3DPos:
                case DataLink.Var2DPosClr:
                case DataLink.Var3DPosClr:
                    bool is3D = (lnk.OutputLabel == DataLink.Var3DPos) || (lnk.OutputLabel == DataLink.Var3DPosClr);
                    int maxRows = Math.Min(bodyList.Count, output.Length);
                    for (int row = 0; row < maxRows; row++) {
                        double[] v = output[row];
                        var b = bodyList[row];
                        double x = (v[0] - DataLink.MapMargin) / lnk.MapFactor;
                        double y = (v[1] - DataLink.MapMargin) / lnk.MapFactor;
                        L1Error += Math.Abs(b.X - x) + Math.Abs(b.Y - y);
                        b.X = x;
                        b.Y = y;
                        if (is3D) {
                            double z = (v[2] - DataLink.MapMargin)/ lnk.MapFactor;
                            L1Error += Math.Abs(b.Z - z);
                            b.Z = z;
                        } else {
                            b.Z = 0;
                        }
                    }

                    if (map != null) {
                        map.MapType = is3D ? "Cube" : "Rectangle";
                        map.Width = lnk.mapWidth;
                        map.Height = lnk.mapHeight;
                        if (is3D) map.Depth = lnk.mapDepth;
                    }

                    if ((lnk.OutputLabel == DataLink.Var2DPosClr) || (lnk.OutputLabel == DataLink.Var3DPosClr)) {
                        short[] index2types = lnk.GetIndex2Type();
                        int idx0 = is3D ? 3 : 2;
                        for (int row = 0; row < maxRows; row++) {
                            int maxIdx = MaxIndex(output[row], idx0) - idx0;
                            
                            if ( bodyList[row].Type != index2types[maxIdx]) {
                                bodyList[row].Type = index2types[maxIdx];
                                mismatches++;
                            }
                        }
                    }
                    break;

                case DataLink.VarColor: {
                        short[] index2types = lnk.GetIndex2Type();
                        for (int row = 0; row < output.Length; row++) {
                            int maxIdx = MaxIndex(output[row], 0);
                            if (bodyList[row].Type != index2types[maxIdx]) {
                                bodyList[row].Type = index2types[maxIdx];
                                mismatches++;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            L1Error /= output.Length;
            return new Tuple<int, double>(mismatches, L1Error);
        }


        void SaveNameList(StreamWriter sw, string name, IList<string> list) {
            if (list == null) return;
            sw.Write(name + ":");
            for (int i = 0; i < list.Count; i++) 
                sw.Write(" " + list[i].Replace(" ", "@@"));
            sw.WriteLine();
        }

        void SaveNumberList(StreamWriter sw, string name, IList<double> list) {
            if (list == null) return;
            sw.Write(name + ":");
            for (int i = 0; i < list.Count; i++)
                sw.Write(" " + list[i].ToString(nmFmt));
            sw.WriteLine();
        }

        public void SaveModelInfo(string name, string modelType) {
            using (StreamWriter sw = new StreamWriter(DataModeling.workDir + name + ".md")) {
                sw.WriteLine("ModelType: " + modelType);
                sw.WriteLine("TrDatasetName: " + TrainingDatasetName);
                sw.WriteLine("TrMapName: " + TrainingMapName);
                sw.WriteLine("TrSamples: " + trSamples);
                sw.WriteLine("TrEpochs: " + TrainingEpochs);
                sw.WriteLine("MapSize: " + mapWidth + " " + mapHeight + " " + mapDepth);

                SaveNameList(sw, "InputVariables", InputVariables);
                SaveNumberList(sw, "InputFactors", InputFactors);
                SaveNumberList(sw, "InputShifts", InputShifts);

                if (outputLabel == DataLink.ColumnList) {
                    SaveNameList(sw, "OutputVariables", outputVariables);
                    SaveNumberList(sw, "OutputFactors", outputFactors);
                    SaveNumberList(sw, "OutputShifts", outputShifts);
                } else if ( (outputLabel !=null) && (outputLabel.StartsWith("<")) ) {
                    sw.WriteLine("OutputVariables: " + outputLabel);
                    if ( outputLabel != DataLink.Nul)
                        sw.WriteLine("MapFactor: " + MapFactor.ToString(nmFmt));
                } else {  // outputVariable is assumed to be a filter name.
                    var app = DataModeling.App.ScriptApp;
                    SaveNameList(sw, "OutputVariables", outputVariables);
                    SaveNumberList(sw, "OutputFactors", outputFactors);
                    SaveNumberList(sw, "OutputShifts", outputShifts);
                }

                short[] idx2Typ = GetIndex2Type();
                if (idx2Typ != null) {
                    sw.Write("Index2Type:");
                    foreach (var tp in idx2Typ)
                        sw.Write(" " + tp);
                    sw.WriteLine();
                }
            }
        }

        public void LoadModelInfo(string filePath) {
            this.modelName = Path.GetFileNameWithoutExtension(filePath);
            this.LastUpdate = File.GetLastWriteTime(filePath);

            using (StreamReader sr = new StreamReader(filePath)) {
                while (true) {
                    string line = sr.ReadLine();
                    if (line == null) break;
                    int idx = line.IndexOf(':');
                    if (idx < 0) continue;
                    string par = line.Substring(idx+1).Trim();
                    string key = line.Substring(0, idx);

                    if (string.IsNullOrEmpty(par))
                        continue;

                    switch (key) {
                        case "ModelType":
                            ModelType = par;
                            break;

                        case "TrDatasetName":
                            TrainingDatasetName = par;
                            break;

                        case "TrMapName":
                            TrainingMapName = par;
                            break;

                        case "TrSamples":
                            trSamples = int.Parse(par);
                            break;

                        case "TrEpochs":
                            TrainingEpochs = int.Parse(par);
                            break;

                        case "MapFactor":
                            MapFactor = double.Parse(par);
                            break;

                        case "MapSize":
                            string[] fs = par.Split(' ');
                            mapWidth = int.Parse(fs[0]);
                            mapHeight = int.Parse(fs[1]);
                            mapDepth = int.Parse(fs[2]);
                            break;

                        case "InputVariables":
                            InputVariables = par.Split(' ').Select(v => v.Trim().Replace("@@", " ")).ToList();
                            break;

                        case "InputFactors":
                            InputFactors = par.Split(' ').Select(v => double.Parse(v)).ToList();
                            break;

                        case "InputShifts":
                            InputShifts = par.Split(' ').Select(v => double.Parse(v)).ToList();
                            break;

                        case "OutputVariables":
                            OutputLabel = par;
                            break;

                        case "OutputFactors":
                            outputFactors = par.Split(' ').Select(v => double.Parse(v)).ToList();
                            break;

                        case "OutputShifts":
                            OutputShifts = par.Split(' ').Select(v => double.Parse(v)).ToList();
                            break;

                        case "Index2Type":
                            var idx2type = par.Split(' ').Select(v => short.Parse(v)).ToList();
                            var type2Index = new Dictionary<short, short>();
                            for (short i = 0; i < idx2type.Count; i++) type2Index[idx2type[i]] = i;
                            Type2Index = type2Index;
                            break;
                    }
                }
            }
        }

        public static void WriteMatrix0(string filePath, double[][] data, string header=null) {
            using (StreamWriter sw = new StreamWriter(filePath)) {
                if ( header != null ) {
                    sw.WriteLine("#" + header);
                }
                foreach (var v in data) {
                    for (int i = 0; i < v.Length; i++) {
                        if (i != 0) sw.Write('|');
                        sw.Write(v[i].ToString("g7"));
                    }
                    sw.WriteLine();
                }
            }
        }

        public static double[][] ReadMatrix0(string filePath) {
            if (!File.Exists(filePath)) return null;

            List<double[]> matrix = new List<double[]>();
            using (StreamReader sr = new StreamReader(filePath)) {
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();
                    if (line != null) {
                        string[] fs = line.Split('|');
                        if (fs.Length > 0)
                            matrix.Add(fs.Select(f => double.Parse(f)).ToArray());
                    }
                }
            }
            return matrix.ToArray();
        }

        public void WriteMatrix(string fileName, double[][] data, string header = null) {
            WriteMatrix0(DataModeling.workDir + fileName + ".csv", data, header);
        }

        public void DeleteDataFile(string fileName) {
            File.Delete(DataModeling.workDir + fileName + ".csv");
        }

        public double[][] ReadMatrix(string fileName) {
            return ReadMatrix0(DataModeling.workDir + fileName + ".csv");
        }

        public class tt {
            public const string Shp = "Shp";
            public const string Clr = "Clr";
            public const string ClrShp = "ClrShp";
            public const string Mdl = "Mdl";
            public const string Var = "Var";
            public const string Nul = "Nul";
            public const string Cls = "Cls";
        };

        public string TargetDescription() {
            var app = DataModeling.App.ScriptApp;
            switch (traningTarget) {
                case tt.Clr:
                    return DataLink.VarColor;

                case tt.Shp:
                    return (app.Map.Dimension == 2) ? DataLink.Var2DPos : DataLink.Var3DPos;

                case tt.ClrShp:
                    return (app.Map.Dimension == 2) ? DataLink.Var2DPosClr : DataLink.Var3DPosClr;

                case tt.Var:
                    return outputGroupName;

                case tt.Mdl:
                    return DataLink.ColumnList;

                case tt.Cls:
                    return DataLink.VarClsType;

                default:
                    return DataLink.Nul;
            }
        }

        public void UnpackModelTargets(string mdInput, string mdOutput) {
            inputGroupName = mdInput;
            if (inputGroupName == "+") {
                inputGroupName = DataModeling.App.ScriptApp.Map.Filter;
            } else if (inputGroupName == "Nul") {
                inputGroupName = DataLink.Nul;
            }

            if (mdOutput.StartsWith(tt.Var)) {
                var fs = mdOutput.Split(':');
                traningTarget = fs[0];
                outputGroupName = fs[1];
                if (outputGroupName == "+")
                    outputGroupName = DataModeling.App.ScriptApp.Map.Filter;
            }
        }

        public string ScanModelScript(string scriptFile, string modelName) {
            this.modelName = modelName;
            string mdTarget = "Nul";
            inputGroupName = DataLink.Nul;
            outputGroupName = "Nul";

            using (StreamReader sr = new StreamReader(DataModeling.workDir + scriptFile)) {
                while (true) {
                    string line = sr.ReadLine();
                    if (line == null) break;
                    if (line.IndexOf("ModelDataset(") >= 0) {
                        string[] fs = line.Split('\'', '\"');
                        if (fs.Length >= 2)
                            inputGroupName = fs[1];
                        if (fs.Length >= 4)
                            mdTarget = fs[3];
                    }
                }
            }

            if ( inputGroupName == "+" ) {
                inputGroupName = DataModeling.App.ScriptApp.Map.Filter;
            } else if (inputGroupName == "Nul") {
                inputGroupName = DataLink.Nul;
            } 

            if (mdTarget.StartsWith(tt.Var)) {
                var fs = mdTarget.Split(':');
                mdTarget = fs[0];
                outputGroupName = fs[1];
                if (outputGroupName == "+")
                    outputGroupName = DataModeling.App.ScriptApp.Map.Filter;
            }

            string[] validTargets = new string[] { tt.Shp, tt.Clr, tt.ClrShp, tt.Mdl, tt.Var, tt.Nul};

            if (!validTargets.Contains(mdTarget)) {
                MessageBox.Show("Invalid Model Script: " + scriptFile + ".\nNo model target specified!");
                return null;
            }

            if (mdTarget == tt.Mdl) {
                mdTarget = ReadModelTarget();
            }

            traningTarget = mdTarget;
            return mdTarget;
        }

        /// <summary>
        /// Read model target from md file for re-training purpose:
        /// </summary>
        /// <returns></returns>
        public string ReadModelTarget() {
            string mdTarget = null;
            // Get the targets from the *.md file.
            try {
                using (StreamReader sr = new StreamReader(DataModeling.workDir + modelName + ".md")) {
                    while (true) {
                        string line = sr.ReadLine();
                        if (line == null) break;
                        int idx = line.IndexOf(':');
                        if (idx < 0) continue;
                        string par = line.Substring(idx + 1).Trim();
                        string key = line.Substring(0, idx);

                        if (string.IsNullOrEmpty(par))
                            continue;

                        switch (key) {
                            case "OutputVariables":
                                if (par.StartsWith("<")) {
                                    switch (par) {
                                        case DataLink.Var2DPos:
                                        case DataLink.Var3DPos:
                                            mdTarget = tt.Shp;
                                            break;
                                        case DataLink.Var2DPosClr:
                                        case DataLink.Var3DPosClr:
                                            mdTarget = tt.ClrShp;
                                            break;
                                        case DataLink.VarColor:
                                            mdTarget = tt.Clr;
                                            break;
                                        case DataLink.Nul:
                                            mdTarget = tt.Nul;
                                            break;
                                    }
                                } else {
                                    outputVariables = par.Split(' ').ToList();
                                }
                                break;
                            case "OutputFactors":
                                outputFactors = par.Split(' ').Select(f => double.Parse(f)).ToList();
                                break;
                            case "OutputShifts":
                                outputShifts = par.Split(' ').Select(f => double.Parse(f)).ToList();
                                break;
                            case "InputVariables":
                                inputVariables = par.Split(' ').ToList();
                                StringBuilder sb = new StringBuilder();
                                foreach (var v in inputVariables) {
                                    if (sb.Length > 0) sb.Append('|');
                                    sb.Append(v);
                                }
                                inputGroupName = sb.ToString();
                                break;
                            case "InputFactors":
                                inputFactors = par.Split(' ').Select(f => double.Parse(f)).ToList();
                                break;
                            case "InputShifts":
                                inputShifts = par.Split(' ').Select(f => double.Parse(f)).ToList();
                                break;
                        }
                    }
                }
            } catch (FileNotFoundException) {
            } catch (ArgumentException) {
            }
            if (string.IsNullOrEmpty(mdTarget)) {
                MessageBox.Show("Cannot determine model target!");
                return null;
            }
            return mdTarget;
        }
    }
}
