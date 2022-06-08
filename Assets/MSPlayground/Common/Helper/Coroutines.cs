
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Common.Helper
{
    /// <summary>
    /// Utility IEnumerator methods to use with coroutines
    /// </summary>
    public static class Coroutines
    {
        /// <summary>
        /// Yields one frame and then invokes callback
        /// </summary>
        /// <param name="onCompleteCallback">Callback</param>
        /// <returns></returns>
        public static IEnumerator WaitOneFrame(System.Action onCompleteCallback)
        {
            yield return null;
            onCompleteCallback.Invoke();
        }
        
        /// <summary>
        /// Yields a certain number of frames and then invokes callback
        /// </summary>
        /// <param name="frames">Number of frames to wait</param>
        /// <param name="onCompleteCallback">Callback</param>
        /// <returns></returns>
        public static IEnumerator WaitFrames(int frames, System.Action onCompleteCallback)
        {
            for (int i = 0; i < frames; ++i)
            {
                yield return null;   
            }
            onCompleteCallback.Invoke();
        }

        /// <summary>
        /// Yields for a certain number of seconds and then invokes callback
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="onCompleteCallback"></param>
        /// <returns></returns>
        public static IEnumerator WaitAfterSeconds(float seconds, System.Action onCompleteCallback)
        {
            yield return new WaitForSeconds(seconds);
            onCompleteCallback?.Invoke();
        }

        /// <summary>
        /// Calls action after waiting end of frame
        /// </summary>
        /// <param name="onCompleteCallback"></param>
        /// <returns></returns>
        public static IEnumerator WaitEndOfFrame(System.Action onCompleteCallback)
        {
            yield return new WaitForEndOfFrame();
            onCompleteCallback?.Invoke();
        }
        
        /// <summary>
        /// Wait until predicate is true before invoking callback
        /// </summary>
        /// <param name="predicate">Bool condition to wait for</param>
        /// <param name="onCompleteCallback">Callback</param>
        /// <returns></returns>
        public static IEnumerator WaitUntil(Func<bool> predicate, System.Action onCompleteCallback)
        {
            yield return new WaitUntil(predicate);
            onCompleteCallback.Invoke();
        }
    }
   
}