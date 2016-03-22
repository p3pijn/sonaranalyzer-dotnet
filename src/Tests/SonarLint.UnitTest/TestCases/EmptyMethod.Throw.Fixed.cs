﻿using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        private EmptyMethod()
        {
        }

        void F2()
        {
            // Do nothing because of X and Y.
        }

        void F3()
        {
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        void F4()    // Noncompliant
        {
            throw new NotSupportedException();
        }

        protected virtual void F5()
        {
        }

        extern void F6();
    }

    public abstract class MyClass
    {
        public void F1()
        {
            throw new NotSupportedException();
        } // Noncompliant

        public abstract void F2();
    }

    public class MyClass2
    {
        public MyClass2() // Noncompliant
        {
        }

        ~MyClass2() // Noncompliant
        {
        }
    }

    public class MyClass3
    {
        static MyClass3() // Noncompliant
        {

        }

        public MyClass3()
        {
        }
        public MyClass3(int i)
        {
        }
    }

    public class MyClass4 : MyClass3
    {
        public MyClass4() : base(10)
        {
        }
    }

    public class MyClass5 : MyClass
    {
        public override void F2()
        {
        }
    }
}
