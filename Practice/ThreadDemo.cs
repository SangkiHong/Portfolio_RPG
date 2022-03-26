using System;
using UnityEngine;
using System.Threading;

namespace SK
{
    public class ThreadDemo : MonoBehaviour
    {
        private void Start()
        {
           //Thread thread = new Thread(Run); // 일반적 생성 방법
           //Thread thread = new Thread(() =>Run()); // 람다식
           /*Thread thread = new Thread(delegate()
           {
               Run();
           }); */ //익명메서드를 이용한 스레드 생성
           //thread.Start();
           new Thread(() => Run()).Start(); // 간단한 표현으로 스레드 생성과 동시에 실행하기

            // 매개변수가 1개면, Start 메서드를 사용할 때 넣어주면 된다.
           Thread thread = new Thread(Run2);
           thread.Start(1); 
        
           // 매개변수가 2가 이상이면, ThreadStart에서 파라미터를 전달하면 된다.
           Thread thread2 = new Thread(() => Sum(1, 2, 3));
           // 일반적으로 스레드는 포그라운드(Foreground)에서 실행되지만 백그라운드에서 실행하고 싶으면 아래와 같이 해주면 된다.
           thread2.IsBackground = true;
           thread2.Start();
        }

        void Run()
        {
            Debug.LogFormat("Thread#{0}: 시작", Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
            Debug.LogFormat("Thread#{0}: 종료", Thread.CurrentThread.ManagedThreadId);
        }

        void Run2(object obj)
        {
            Debug.Log(obj);
        }

        static void Sum(int d1, int d2, int d3)
        {
            int sum = d1 + d2 + d3;
            Debug.Log(sum);
        }
    }
}