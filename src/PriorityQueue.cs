using System;
using System.Collections;
using System.Collections.Generic;

namespace Suroden
{
	public interface IPriorityQueue<T> : ICollection<T>, /*ICloneable,*/ IList<T>
	{
		int Push(T O);
		T Pop();
		T Peek();
		void Update(int i);
	}
	public class BinaryPriorityQueue<T> : IPriorityQueue<T>, ICollection<T>, /*ICloneable,*/ IList<T>
	{
		protected List<T> InnerList = new List<T>();
		protected IComparer<T> Comparer;

		#region contructors
		public BinaryPriorityQueue() : this(System.Collections.Generic.Comparer<T>.Default)
		{}
		public BinaryPriorityQueue(IComparer<T> c)
		{
			Comparer = c;
		}
		public BinaryPriorityQueue(int C) : this(System.Collections.Generic.Comparer<T>.Default,C)
		{}
		public BinaryPriorityQueue(IComparer<T> c, int Capacity)
		{
			Comparer = c;
			InnerList.Capacity = Capacity;
		}

		/*protected BinaryPriorityQueue(List<T> Core, IComparer<T> Comp, bool Copy)
		{
			if(Copy)
				InnerList = Core.Clone() as List<T>;
			else
				InnerList = Core;
			Comparer = Comp;
		}*/

		#endregion
		protected void SwitchElements(int i, int j)
		{
			T h = InnerList[i];
			InnerList[i] = InnerList[j];
			InnerList[j] = h;
		}

		protected virtual int OnCompare(int i, int j)
		{
			return Comparer.Compare(InnerList[i],InnerList[j]);
		}

		#region public methods
		/// <summary>
		/// Push an object onto the PQ
		/// </summary>
		/// <param name="O">The new object</param>
		/// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
		public int Push(T O)
		{
			int p = InnerList.Count,p2;
			InnerList.Add(O); // E[p] = O
			do
			{
				if(p==0)
					break;
				p2 = (p-1)/2;
				if(OnCompare(p,p2)<0)
				{
					SwitchElements(p,p2);
					p = p2;
				}
				else
					break;
			}while(true);
			return p;
		}

		/// <summary>
		/// Get the smallest object and remove it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Pop()
		{
			T result = InnerList[0];
			int p = 0,p1,p2,pn;
			InnerList[0] = InnerList[InnerList.Count-1];
			InnerList.RemoveAt(InnerList.Count-1);
			do
			{
				pn = p;
				p1 = 2*p+1;
				p2 = 2*p+2;
				if(InnerList.Count>p1 && OnCompare(p,p1)>0) // links kleiner
					p = p1;
				if(InnerList.Count>p2 && OnCompare(p,p2)>0) // rechts noch kleiner
					p = p2;
				
				if(p==pn)
					break;
				SwitchElements(p,pn);
			}while(true);
			return result;
		}

		/// <summary>
		/// Notify the PQ that the object at position i has changed
		/// and the PQ needs to restore order.
		/// Since you dont have access to any indexes (except by using the
		/// explicit IList.this) you should not call this function without knowing exactly
		/// what you do.
		/// </summary>
		/// <param name="i">The index of the changed object.</param>
		public void Update(int i)
		{
			int p = i,pn;
			int p1,p2;
			do	// aufsteigen
			{
				if(p==0)
					break;
				p2 = (p-1)/2;
				if(OnCompare(p,p2)<0)
				{
					SwitchElements(p,p2);
					p = p2;
				}
				else
					break;
			}while(true);
			if(p<i)
				return;
			do	   // absteigen
			{
				pn = p;
				p1 = 2*p+1;
				p2 = 2*p+2;
				if(InnerList.Count>p1 && OnCompare(p,p1)>0) // links kleiner
					p = p1;
				if(InnerList.Count>p2 && OnCompare(p,p2)>0) // rechts noch kleiner
					p = p2;
				
				if(p==pn)
					break;
				SwitchElements(p,pn);
			}while(true);
		}

		/// <summary>
		/// Get the smallest object without removing it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Peek()
		{
			if(InnerList.Count>0)
				return InnerList[0];
			else
				return default(T);
		}

		public bool Contains(T value)
		{
			return InnerList.Contains(value);
		}

		public void Clear()
		{
			InnerList.Clear();
		}

		public int Count
		{
			get
			{
				return InnerList.Count;
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}

		public void CopyTo(T[] array, int index)
		{
			InnerList.CopyTo(array,index);
		}

		/*public object Clone()
		{
			return new BinaryPriorityQueue<T>(InnerList,Comparer,true);	
		}*/

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}
		#endregion
		#region explicit implementation
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public T this[int index]
		{
			get
			{
				return InnerList[index];
			}
			set
			{
				InnerList[index] = value;
				Update(index);
			}
		}

		public void Add(T o)
		{
			Push(o);
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, T value)
		{
			throw new NotSupportedException();
		}

		public bool Remove(T value)
		{
			throw new NotSupportedException();
		}

		public int IndexOf(T value)
		{
			throw new NotSupportedException();
		}

		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/*public static BinaryPriorityQueue<T> Syncronized(BinaryPriorityQueue<T> P)
		{
			return new BinaryPriorityQueue<T>(List<T>.Synchronized(P.InnerList),P.Comparer,false);
		}
		public static BinaryPriorityQueue<T> ReadOnly(BinaryPriorityQueue<T> P)
		{
			return new BinaryPriorityQueue<T>(List<T>.ReadOnly(P.InnerList),P.Comparer,false);
		}*/
		#endregion
	}
}
