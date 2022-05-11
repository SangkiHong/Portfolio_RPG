namespace SK.Utilities
{
    public class MyMath
    {
        // 싱글톤 Instance
        private static MyMath _instance;
        public static MyMath Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MyMath();
                return _instance;
            }
        }
        private readonly float[] sineLookupTable;

        public MyMath()
        {
            // Sine Lookup Table 초기화
            sineLookupTable = new float[360];

            float radian;
            float deg2rad = UnityEngine.Mathf.Deg2Rad;

            // Sine Lookup Table에 1~360degree에 대한 Sine 값 저장
            for (int i = 0; i < 360; i++)
            {
                if (i + 1 == 180 || i + 1 == 360)
                    radian = 0;
                else
                    radian = deg2rad * (i + 1);
                sineLookupTable[i] = UnityEngine.Mathf.Sin(radian);
            }
        }

        public float GetSine(uint degree)
        {
            // 1~360degree에 대한 Sine 값 반환
            if (0 < degree && sineLookupTable.Length > degree)
                return sineLookupTable[degree];
            else
                return sineLookupTable[degree % 360];
        }
    }
}
