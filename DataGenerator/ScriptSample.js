// File: ScriptSample.js
//
// Description: This script contains sample code to generate
// variou type of datasets with the DataGenerator plugin.
//
var dg = vv.FindPluginObject("DataGenerator")
var ds; // variable to hold a dataset object.

//
// Double click a number in the following block to use spinor to probe different settings.
//
switch( 0 ) {  //0:14
	case 0: ds = dg.Rectangle(0, 10, 20);  // Rectangle(int type, int width, int height);
		break;
	case 1: ds = dg.Circle(0, 10, 20);   // Circle(int type, int width, int height);
		break;
	case 2: ds = dg.Disc(0, 12, 1.5);  // Disc(int type, double R, double length)
		break;
	case 3: ds = dg.Line(0, 400, 25) // Line(int type, double length, int number)
		break;
	case 4: ds = dg.RandomBall(0, 2.5, 1000); // RandomBall(int type, double radius, int bodyNumber)
		break;
	case 5: ds = dg.RandomCubic(0, 100, 1000); // RandomCubic(int type, double edgeLength, int totalNumber)
		break;
	case 6: ds = dg.SingleHelix(0, 5, 0.25, Math.PI/24.0, 12*Math.PI);  // SingleHelix(int type, double r, double delta, double alpha, double alphaTotal)
		break;
	case 7: ds = dg.Sphere(0, 50.0, 0.1);  // Sphere(int type, double R, double alpha)
		break;
	case 8: ds = dg.Spheroid(0, 10.0, 5.0, 0.505);  // Spheroid(int type, double rMajor, double rMinor, double arcLen)
		break;
	case 9: ds = dg.Torus(2, 1, 80, 40); // Torus(double R, double r, int N, int n)
		break;
	case 10: ds = dg.Triangle(0, 20.0);  // Triangle(int type, double edgeLength)
		break;
	case 11: ds = dg.OpenBox(20);  // OpenBox(int edgeSize)
		break;
	case 12: ds = dg.KleinBottle4D(70, 70);  // KleinBottle4D(int width, int height)
		break;
	case 13: ds = dg.Projective4D(10, Math.PI / 108);  // Projective4D(double R, double alpha)
		break;
	case 14: ds = dg.Gaussian(0, 1000); // 3D Gaussian sampling.
		break;
}


/*
// 
// More complex usage:
//

// ds.Scale(1.1, 1.1, 1.1).Rotate(0.1, 'x').AddBodySet(ds.Clone().SetType(15).Translate(50, 0, 0)).Show();

ds = dg.Empty();
for(var w=0; w<6*Math.PI; w+=0.05) 
  ds.AddPoint(0, w, Math.sin(w), Math.cos(w));

for(var i=0; i<ds.Bodies.Count; i++) {
  ds.Bodies[i].Type = Math.round(i / (ds.Bodies.Count/15));
}
*/

var vw = ds.Show();  // show the dataset in a 3D data view.
vw.Title = "Data Points: " + vw.GetNumberTable().Rows;
vw.ResetView();
for(var i=0; i<50; i++) vw.RotateXYZ(-0.02, -0.02, 0);
vv.EventSource.Item.Select(); // Activate the script editor again for further tests.
