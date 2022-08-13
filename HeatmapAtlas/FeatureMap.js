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

	public Bitmap ArrayImageFast(double[] values) {
	  var bm = new Bitmap(28, 28);
     var clr = Color.FromArgb(200, Color.Black);

     BitmapData data = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), 
			ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
     int stride = data.Stride;
     unsafe {
            byte* ptr = (byte*)data.Scan0;
			   for(int i=0; i<values.Length; i++) {
			     if ( values[i] > 0 ) {
                 int i0 = (i%28)*4 + (i/28)*stride;
					  ptr[i0] = clr.B;
					  ptr[i0+1] = clr.G;
					  ptr[i0+2] = clr.R;
					  ptr[i0+3] = clr.A;
              }
			  }
		}
		bm.UnlockBits(data);
	   return bm;
	}   

	public void SetImages(IList<ISiteItem> items) {
	   var ds = vv.Dataset;
		double[][] M = ds.GetNumberTableView().Matrix as double[][];
      MT.ForEach(items, item=>{
			var imgItem = item as ISiteImageItem;
			var rowIdx = ds.BodyIndexForId(imgItem.Id);
	      imgItem.Image = ArrayImageFast( M[rowIdx] );
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
`);


var sm = New.SiteMap('TestA');
sm.BackColor = New.Color("Black");
sm.Clear();
cs.AddBodyImages(sm, vv.Map.SelectedBodies);
cs.SetImages2(sm.Items, 0.5);
sm.Show();

