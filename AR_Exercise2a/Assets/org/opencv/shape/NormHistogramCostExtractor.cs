﻿
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity.ShapeModule
{

    // C++: class NormHistogramCostExtractor
    //javadoc: NormHistogramCostExtractor

    public class NormHistogramCostExtractor : HistogramCostExtractor
    {

        protected override void Dispose (bool disposing)
        {
#if ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
try {
if (disposing) {
}
if (IsEnabledDispose) {
if (nativeObj != IntPtr.Zero)
shape_NormHistogramCostExtractor_delete(nativeObj);
nativeObj = IntPtr.Zero;
}
} finally {
base.Dispose (disposing);
}
#else
            return;
#endif
        }

        protected internal NormHistogramCostExtractor (IntPtr addr) : base (addr) { }

        // internal usage only
        public static new NormHistogramCostExtractor __fromPtr__ (IntPtr addr) { return new NormHistogramCostExtractor (addr); }

        //
        // C++:  int cv::NormHistogramCostExtractor::getNormFlag()
        //

        //javadoc: NormHistogramCostExtractor::getNormFlag()
        public int getNormFlag ()
        {
            ThrowIfDisposed ();
#if ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
        int retVal = shape_NormHistogramCostExtractor_getNormFlag_10(nativeObj);
        
        return retVal;
#else
            return -1;
#endif
        }


        //
        // C++:  void cv::NormHistogramCostExtractor::setNormFlag(int flag)
        //

        //javadoc: NormHistogramCostExtractor::setNormFlag(flag)
        public void setNormFlag (int flag)
        {
            ThrowIfDisposed ();
#if ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
        shape_NormHistogramCostExtractor_setNormFlag_10(nativeObj, flag);
        
        return;
#else
            return;
#endif
        }


#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
#else
        const string LIBNAME = "opencvforunity";
#endif



        // C++:  int cv::NormHistogramCostExtractor::getNormFlag()
        [DllImport (LIBNAME)]
        private static extern int shape_NormHistogramCostExtractor_getNormFlag_10 (IntPtr nativeObj);

        // C++:  void cv::NormHistogramCostExtractor::setNormFlag(int flag)
        [DllImport (LIBNAME)]
        private static extern void shape_NormHistogramCostExtractor_setNormFlag_10 (IntPtr nativeObj, int flag);

        // native support for java finalize()
        [DllImport (LIBNAME)]
        private static extern void shape_NormHistogramCostExtractor_delete (IntPtr nativeObj);

    }
}
