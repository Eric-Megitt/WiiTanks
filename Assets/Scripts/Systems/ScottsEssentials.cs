using System;
using UnityEngine;
using Unity.Mathematics;

namespace ScottsEssentials
{
	public static class ConversionTools
	{
		public static float2 Vector2ToFloat2(Vector2 vec2) => new float2(vec2.x, vec2.y);
		public static Vector2 Float2ToVector2(float2 float2) => new Vector2(float2.x, float2.y);
	}

	public static class PsuedoRandom
	{
		public static bool Bool => new System.Random().Next(2) == 0;
		public static void Shuffle<T>(this T[] array)
		{
			System.Random rng = new();
			int n = array.Length;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}
		}
	}

	public static class ArrayTools
	{
		public static void ResetArray<T>(this T[] array)
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = default(T);
		}
	}

	public static class Maths
	{
		public static float[] ExponentialGraph(int duration, float exponent)
		{
			float[] graph = new float[duration];
			for (int i = 1; i < duration; i++)
				graph[i] = Mathf.Pow((float)i / duration, exponent);

			return graph;
		}

		public static float Sq(float baseComponent) => baseComponent * baseComponent;
	}
	public static class VectorExtensions
	{
		public static Vector2 ToVector2(this Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static Vector3 ToVector3(this Vector2 v)
		{
			return new Vector3(v.x, v.y, 0.0f);
		}

		public static Vector3 Vector3(Vector2 ab, float c) => new Vector3(ab.x, ab.y, c);
	}

	#region SingeltonReworked
	[DefaultExecutionOrder(-2)]
	public class Singleton<T> : MonoBehaviour where T : Component
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					Debug.LogError("No object of type " + typeof(T).FullName + " was found.");
					return null;
				}

				return instance;
			}
		}

		/// <summary>
		/// Use <see cref="WakeUp()"/> instead, otherwise make sure to run base.Awake()
		/// </summary>
		protected virtual void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
			}
			else
			{
				Destroy(this.gameObject);
			}
			WakeUp();
		}

		/// <summary>
		/// Is called at the end of base.Awake()
		/// 
		/// <para>[DefaultExecutionOrder(-2)]</para>
		/// </summary>
		protected virtual void WakeUp() { }

		private void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}
	}

	[DefaultExecutionOrder(-2)]
	public class SingletonPersistent<T> : MonoBehaviour where T : Component
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					Debug.LogError("No object of type " + typeof(T).FullName + " was found.");
					return null;
				}

				return instance;
			}
		}

		/// <summary>
		/// Use <see cref="WakeUp()"/> instead, otherwise make sure to run base.Awake()
		/// </summary>
		protected virtual void Awake()
		{
			if (instance == null)
			{
				instance = this as T;
			}
			else
			{
				Destroy(this.gameObject);
			}
			WakeUp();
		}

		/// <summary>
		/// Is called at the end of base.Awake()
		/// 
		/// <para>[DefaultExecutionOrder(-2)]</para>
		/// </summary>
		protected virtual void WakeUp() { }

		private void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}
	}
	#endregion SingletonReworked
}