using System;
using System.Collections;
using System.Collections.Generic;

namespace MIDIController.Serial {
    public class ConcurrentCircularBuffer<E> : IEnumerable<E> {
        private readonly E[] buffer;
        private readonly int capacity;
        private int index;

        public int Capacity => this.capacity;

        /// <summary>
        /// Creates an instance of the concurrent wrapping buffer
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="emptyPredicate">A predicate that can tell if a value is null/empty (e.g if it's a structure type)</param>
        public ConcurrentCircularBuffer(int capacity) {
            this.buffer = new E[capacity];
            this.capacity = capacity;
            this.index = 0;
        }

        public void Push(E value) {
            lock (this) {
                if (this.index == this.capacity) {
                    this.index = 0;
                }

                this.buffer[this.index++] = value;
            }
        }

        public E Pop() {
            lock (this) {
                int index = this.index;
                if (index == 0) {
                    this.index = this.capacity;
                    return (E) this.buffer[0];
                }
                else {
                    return (E) this.buffer[this.index = index - 1];
                }
            }
        }

        public void PushAll(params E[] values) {
            if (values.Length > this.capacity) {
                throw new ArgumentException("Cannot add all elements because the source array is bigger than the internal array");
            }
            else {
                // cant be bothered to do it property
                foreach(E element in values) {
                    this.Push(element);
                }
            }
        }

        public E Get(int index) {
            if (index < 0 || index > this.capacity) {
                throw new IndexOutOfRangeException("Cannot get an element because the index out of bounds! " + index + " < 0 || " + index + " > " + this.capacity);
            }

            lock (this) {
                return this.buffer[index];
            }
        }

        public E UnsafeGet(int index) {
            return this.buffer[index];
        }

        public void Set(int index, E value) {
            if (index < 0 || index > this.capacity) {
                throw new IndexOutOfRangeException("Cannot set an element because the index out of bounds! " + index + " < 0 || " + index + " > " + this.capacity);
            }

            lock (this) {
                this.buffer[index] = value;
            }
        }

        public void UnsafeSet(int index, E value) {
            this.buffer[index] = value;
        }

        public void Swap(int oldIndex, int newIndex) {
            if (oldIndex == newIndex) {
                throw new Exception("oldIndex and newIndex cannot be the same");
            }

            if (oldIndex < 0 || newIndex < 0 || oldIndex > this.capacity || newIndex > this.capacity) {
                throw new IndexOutOfRangeException("Cannot swap element because one of the indexes are out of bounds!");
            }

            lock (this) {
                E[] buffer = this.buffer;
                E oldObj = buffer[oldIndex];
                E newObj = buffer[newIndex];
                buffer[oldIndex] = newObj;
                buffer[newIndex] = oldObj;
            }
        }

        public void Reset() {
            lock (this) {
                this.index = 0;
            }
        }

        public void Clear() {
            lock (this) {
                E[] objects = this.buffer;
                for (int i = 0, len = objects.Length; i < len; i++) {
                    objects[i] = default;
                }

                this.index = 0;
            }
        }

        public IEnumerator<E> GetEnumerator() {
            lock(this) {
                E[] buffer = this.buffer;
                for(int i = this.index, end = this.capacity; i < end; i++) {
                    yield return buffer[i];
                }
                for(int i = 0, end = Math.Min(this.capacity, this.index); i < end; i++) {
                    E e = buffer[i];
                    if (e != null) {
                        yield return e;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            lock (this) {
                E[] buffer = this.buffer;
                for (int i = this.index, end = this.capacity; i < end; i++) {
                    yield return buffer[i];
                }
                for (int i = 0, end = Math.Min(this.capacity, this.index); i < end; i++) {
                    E e = buffer[i];
                    if (e != null) {
                        yield return e;
                    }
                }
            }
        }
    }
}