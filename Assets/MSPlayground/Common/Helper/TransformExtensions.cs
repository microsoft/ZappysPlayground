using System;
using System.Collections;
using UnityEngine;

namespace MSPlayground.Common
{
    /// <summary>
    /// Utility extension class for Transform
    /// </summary>
    public static partial class TransformExtensions
    {
        /// <summary>
        /// Scales the target transform over a certain amount of time
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tweenData"></param>
        /// <param name="onCompletionCallback"></param>
        /// <returns></returns>
        public static IEnumerator TweenScale(this Transform target, Tween<Vector3> tweenData, Action onCompletionCallback = null)
        {
            target.localScale = tweenData.From;
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                if (tweenData.Curve != null)
                {
                    target.localScale = Vector3.Lerp(tweenData.From, tweenData.To, tweenData.Curve.Evaluate(timer / tweenData.Duration));
                }
                else
                {
                    target.localScale = Vector3.Lerp(tweenData.From, tweenData.To, timer / tweenData.Duration);
                }

                yield return null;
            } while (timer <= tweenData.Duration);

            target.localScale = tweenData.To;
            onCompletionCallback?.Invoke();     
        }
        
        /// <summary>
        /// Scales the target transform over a certain amount of time
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tweenData"></param>
        /// <param name="onCompletionCallback"></param>
        /// <returns></returns>
        public static IEnumerator TweenScaleReverse(this Transform target, Tween<Vector3> tweenData, Action onCompletionCallback = null)
        {
            target.localScale = tweenData.To;
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                if (tweenData.Curve != null)
                {
                    target.localScale = Vector3.Lerp(tweenData.To, tweenData.From, tweenData.Curve.Evaluate(timer / tweenData.Duration));
                }
                else
                {
                    target.localScale = Vector3.Lerp(tweenData.To, tweenData.From, timer / tweenData.Duration);
                }

                yield return null;
            } while (timer <= tweenData.Duration);

            target.localScale = tweenData.From;
            onCompletionCallback?.Invoke();     
        }
        
        /// <summary>
        /// Scales the target transform over a certain amount of time
        /// </summary>
        /// <param name="target">Transform that will be modified over time</param>
        /// <param name="from">Staring value</param>
        /// <param name="to">Final value</param>
        /// <param name="duration">Time that the tween will take</param>
        /// <param name="animationCurve">Optional animation curve</param>
        /// <param name="onCompletionCallback">Optional callback</param>
        /// <returns></returns>
        public static IEnumerator TweenScale(this Transform target,
            Vector3 from,
            Vector3 to,
            float duration,
            AnimationCurve animationCurve = null,
            Action onCompletionCallback = null)
        {
            target.localScale = from;
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                if (animationCurve != null)
                {
                    target.localScale = Vector3.Lerp(from, to, animationCurve.Evaluate(timer / duration));
                }
                else
                {
                    target.localScale = Vector3.Lerp(from, to, timer / duration);
                }

                yield return null;
            } while (timer <= duration);

            target.localScale = to;

            onCompletionCallback?.Invoke();
        }

        /// <summary>
        /// Translates the target transform over a certain amount of time
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tweenData"></param>
        /// <param name="space"></param>
        /// <param name="onCompletionCallback"></param>
        /// <returns></returns>
        public static IEnumerator TweenPosition(this Transform target, Tween<Vector3> tweenData, Space space, Action onCompletionCallback = null)
        {
            SetPosition(target, tweenData.From, space);
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                CalculateAndSetPosition(target, tweenData.From, tweenData.To, space, timer, tweenData.Duration, tweenData.Curve);
                yield return null;
            } while (timer <= tweenData.Duration);

            onCompletionCallback?.Invoke();
        }

        /// <summary>
        /// Translates the target transform over a certain amount of time
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tweenData"></param>
        /// <param name="space"></param>
        /// <param name="onCompletionCallback"></param>
        /// <returns></returns>
        public static IEnumerator TweenPositionReverse(this Transform target, Tween<Vector3> tweenData, Space space, Action onCompletionCallback = null)
        {
            SetPosition(target, tweenData.To, space);
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                CalculateAndSetPosition(target, tweenData.To, tweenData.From, space, timer, tweenData.Duration, tweenData.Curve);
                yield return null;
            } while (timer <= tweenData.Duration);

            onCompletionCallback?.Invoke();
        }

        /// <summary>
        /// Translates the target transform over a certain amount of time
        /// </summary>
        /// <param name="target">Transform that will be modified over time</param>
        /// <param name="from">Staring value</param>
        /// <param name="to">Final value</param>
        /// <param name="duration">Time that the tween will take</param>
        /// <param name="animationCurve">Optional animation curve</param>
        /// <param name="onCompletionCallback">Optional callback</param>
        /// <returns></returns>
        public static IEnumerator TweenPosition(this Transform target,
            Vector3 from,
            Vector3 to,
            Space space,
            float duration,
            AnimationCurve animationCurve = null,
            Action onCompletionCallback = null)
        {
            SetPosition(target, from, space);
            float timer = 0.0f;
            do
            {
                timer += Time.deltaTime;
                CalculateAndSetPosition(target, from, to, space, timer, duration, animationCurve);
                yield return null;
            } while (timer <= duration);

            onCompletionCallback?.Invoke();
        }

        /// <summary>
        /// Function called each frame during position coroutines
        /// </summary>
        private static void CalculateAndSetPosition(this Transform target, Vector3 from, Vector3 to, Space space, float timer, float duration, AnimationCurve animationCurve)
        {
            float ratio = Mathf.Clamp01(timer / duration);
            ratio = animationCurve == null ? ratio : animationCurve.Evaluate(ratio);
            SetPosition(target, Vector3.LerpUnclamped(from, to, ratio), space);
        }

        /// <summary>
        /// Set world or local position
        /// </summary>
        private static void SetPosition(this Transform target, Vector3 position, Space space)
        {
            if (space == Space.Self)
            {
                target.localPosition = position;
            }
            else
            {
                target.position = position;
            }
        }
    }
}