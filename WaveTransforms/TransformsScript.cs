/// <copyright from="2004" to="2010" company="VisuMap Technologies Inc.">
///   Copyright (C) VisuMap Technologies Inc.
/// 
///   Permission to use, copy, modify, distribute and sell this 
///   software and its documentation for any purpose is hereby 
///   granted without fee, provided that the above copyright notice 
///   appear in all copies and that both that copyright notice and 
///   this permission notice appear in supporting documentation. 
///   VisuMap Technologies Company makes no representations about the 
///   suitability of this software for any purpose. It is provided 
///   "as is" without explicit or implied warranty. 
/// </copyright>
using System;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.WaveTransforms {
    public class TransformsScript : MarshalByRefObject, IPluginObject {
        public FourierTransform NewFourier(int dimension) {
            return new FourierTransform(dimension);
        }

        public HaarTransform NewHaar(int dimension) {
            return new HaarTransform(dimension);
        }

        public WalshTransform NewWalsh(int dimension) {
            return new WalshTransform(dimension);
        }

        public WaveletD4Transform NewWaveletD4(int dimension) {
            return new WaveletD4Transform(dimension);
        }

        public PcaTransform NewPca(INumberTable numberTable) {
            return new PcaTransform(numberTable);
        }

        public string Name {
            get { return "WaveTransforms"; }
            set { }
        }        
    }
}
