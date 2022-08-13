var cs = New.CsObject(`
	public Bitmap ArrayImage(double[] values) {
	  var bm = new Bitmap(28, 28);
     var clr = Color.FromArgb(200, Color.Black);
	  for(int i=0; i<values.Length; i++) {
	    if ( values[i] > 0 )
	      bm.SetPixel(i%28, i/28, clr);
	  }
	  return bm;
	}

	public void SetImagesMNIST(IList<ISiteItem> items, float factor) {
	   var ds = vv.Dataset;
		double[][] M = ds.GetNumberTableView().Matrix as double[][];
      MT.ForEach(items, item=>{
			var imgItem = item as ISiteImageItem;
			var rowIdx = ds.BodyIndexForId(imgItem.Id);
	      imgItem.Image = ArrayImage( M[rowIdx] );
			imgItem.Factor = factor;
		});
	}

	public Bitmap ArrayImage2(double[] values, int bmWidth, int bmHeight, double minValue, double scale) {
	  var bm = new Bitmap(bmWidth, bmHeight);
	  using(Graphics g = Graphics.FromImage(bm)) g.Clear(Color.Transparent);
     for(int i=0; i<values.Length; i++) {
		   int v = (int) ((values[i] - minValue) * scale);
	      if ( v > 0 ) {
           v = 255 - Math.Min(255, v);
			  Color clr = Color.FromArgb(255, v, v, v);
	        bm.SetPixel(i%bmWidth, i/bmHeight, clr);
		   }
	  }
	  return bm;
	}

	public void SetImages2(IList<ISiteItem> items, float factor) {
	   var ds = vv.Dataset;
		var nt = ds.GetNumberTableView();
		int bmWidth = (int) Math.Sqrt(nt.Columns);
		int bmHeight = (int) Math.Ceiling((double)nt.Columns/bmWidth);
		double minValue = nt.MinimumValue();
		double maxValue = nt.MaximumValue();
		double scale = 255.0/(maxValue - minValue);

		double[][] M = ds.GetNumberTableView().Matrix as double[][];

      MT.ForEach(items, item=>{
			var imgItem = item as ISiteImageItem;
			var rowIdx = ds.BodyIndexForId(imgItem.Id);
	      imgItem.Image = ArrayImage2( M[rowIdx], bmWidth, bmHeight, minValue, scale );
			imgItem.Factor = factor;
		});
	}

	public Bitmap ArrayImage3(double[] values, IList<float> xy, double minValue, double scale, double mapSize) {
	  int mSz = (int) mapSize;
	  var bm = new Bitmap(mSz, mSz);
	  using(Graphics g = Graphics.FromImage(bm)) {
	     g.Clear(Color.Transparent);
	     for(int i=0; i<values.Length; i++) {
			   int v = (int) ((values[i] - minValue) * scale);
		      if ( v > 0 ) {
	           v = 255 - Math.Min(255, v);
				  Color clr = Color.FromArgb(255, v, v, v);
				  using(Brush brush = new SolidBrush(clr))
		          g.FillRectangle(brush, mSz*xy[2*i], mSz*xy[2*i+1], 1.0f, 1.0f);
			   }
		  }
	  }
	  return bm;
	}

	public void SetImages3(IList<ISiteItem> items, IList<float> xy, float mapSize) {
	   var ds = vv.Dataset;
		var nt = ds.GetNumberTableView();
		double minValue = nt.MinimumValue();
		double maxValue = nt.MaximumValue();
		double scale = 255.0/(maxValue - minValue);

		double[][] M = ds.GetNumberTableView().Matrix as double[][];

      MT.ForEach(items, item=>{
			var imgItem = item as ISiteImageItem;
			var rowIdx = ds.BodyIndexForId(imgItem.Id);
	      imgItem.Image = ArrayImage3( M[rowIdx], xy, minValue, scale, mapSize);
		});
	}

   public void AddBodyImages(ISiteMap sm, IList<IBody> bodyList) {
      var itemList = sm.Items;
		foreach(IBody b in bodyList) {
			var it = sm.NewImageItem();
			it.Id = b.Id;
			it.X = (float)b.X;
			it.Y = (float)b.Y;
			itemList.Add(it);
		}
   }

   public float[] GetFeatureCfg(string atlasName, string fmItemId) {
		var fm = vv.AtlasManager.OpenMap(atlasName, fmItemId);
		var bs = fm.BodyList;
		float[] xy = new float[2*bs.Count];
		float mWidth = fm.MapLayout.Width;
		float mHeight = fm.MapLayout.Height;
		for(var k=0; k<bs.Count; k++) {
			xy[2*k] = (float)(bs[k].X/mWidth);
		   xy[2*k+1] = (float)(bs[k].Y/mHeight);
		}
		fm.Close();
	   return xy;
   }

`);

function SetImages3(sm) {
	var xy = vv.GetObject('xy');
	if ( xy == null ) {
	  xy = cs.GetFeatureCfg('FeatureMaps', 'FM1');
	  vv.SetObject('xy', xy);
	}
	cs.SetImages3(sm.Items, xy, 50.0);
}


var sm = New.SiteMap('<Temp>');
sm.ReadOnly = false;
sm.BackColor = New.Color("Black");
sm.Clear();
cs.AddBodyImages(sm, vv.Map.SelectedBodies);


//SetImages3(sm);
//cs.SetImages2(sm.Items, 0.5);
cs.SetImagesMNIST(sm.Items, 0.5); sm.BackColor = New.Color("White");

sm.Show();


