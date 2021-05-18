// ReadWigData.js
var cs = New.CsObject("\
	public float[] ReadWigData(string fileName, string chrName, int maxSize) {\
		float[] values = new float[maxSize];\
		using(var sr = new StreamReader(fileName)) {\
			while(true) {\
				string line = sr.ReadLine();\
				if ( line == null ) break;\
				var fs = line.Split(new char[]{'\t', ' '});\
				if ( (fs.Length == 4) && (fs[0] == chrName) ) {\
					int beginIdx = int.Parse(fs[1]);\
					int endIdx = int.Parse(fs[2]);\
					if ( endIdx <= maxSize ) {\
						for(int i=beginIdx; i<endIdx; i++) {\
							values[i] = float.Parse(fs[3]);\
						}\
					}\
				}\
			}\
		}\
		return values;\
	}\
	public float[] ReadFloats(IBlob blob) {\
		var br = new BinaryReader(blob.Stream);\
		string[] fs = blob.ContentType.Split(' ');\
		int valuesLen = int.Parse(fs[2]);\
		float[] values = new float[valuesLen];\
		for(int i=0; i<values.Length; i++)\
			values[i] = br.ReadSingle();\
		return values;\
	}\
\
");

function ReadBlob(fName) {
	var blob = vv.Folder.OpenBlob(fName, false);
	var values = cs.ReadFloats(blob);
	blob.Close();
	return values;
}

function ShowValues(values, fileName, chrName) {
	var bb = New.BigBarView(values);
	bb.Show();
	bb.Title = fileName + " | " + chrName
}

var fName = "C13M17";
var fileName = "C:/temp/" + fName + ".wig";
var chrName = "chr13";
var seqLength = 114350356;


/*
var fName = "ENCFF269TPE";
var fileName = "C:/work/VisuMapPlugin/GeneticAnalysis/" + fName + ".wig";
var chrName = "chr2L";
var seqLength = 22984346;

var fName = "ENCFF136DFJ"; //ENCFF921NSL|ENCFF136DFJ|ENCFF747QBF
var fileName = "c:/Users/JamesLi/Desktop/C.Elegan/" + fName + ".wig";
var chrName = "chrI";
var seqLength = 15072434;

var fName = "ENCFF759HEF"; //ENCFF269TPE|ENCFF759HEF
var fileName = "c:/Users/JamesLi/Desktop/Fruit Fly/" + fName + ".wig";
var seqLength = 23513750;
var chrName = "chr2L";
*/

var values = cs.ReadWigData(fileName, chrName, seqLength);
//var values = cs.ReadBlob(fName);

//ShowValues(values, fileName, chrName);
var bb = New.BigBarView(values);
bb.SaveValuesAsBlob(values, fName, 0, chrName);
bb.Show();
