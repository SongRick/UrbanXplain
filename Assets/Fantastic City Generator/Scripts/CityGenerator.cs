using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace FCG
{
    public class CityGenerator : MonoBehaviour
    {

        // 用于存储一个整数，具体用途可能与建筑数量、索引等相关，初始值为 0
        private int nB = 0;
        // 存储城市的中心点坐标，用于确定城市的位置和布局
        private Vector3 center;
        // 用于记录住宅相关的数量，具体用途可能与住宅建筑数量统计有关，初始值为 0
        private int residential = 0;
        // 布尔类型变量，用于标记是否存在住宅相关的状态，初始值为 false
        private bool _residential = false;

        // 用于存储代表城市的游戏对象，可用于管理城市的创建、销毁等操作
        GameObject cityMaker;

        // 以下一系列数组用于存储不同类型的边界游戏对象
        // [HideInInspector] 表示这些数组在 Unity 编辑器的 Inspector 面板中不可见
        // 迷你边界游戏对象数组
        [HideInInspector]
        public GameObject[] miniBorder;

        // 小边界游戏对象数组
        [HideInInspector]
        public GameObject[] smallBorder;

        // 中等边界游戏对象数组
        [HideInInspector]
        public GameObject[] mediumBorder;

        // 大边界游戏对象数组
        [HideInInspector]
        public GameObject[] largeBorder;

        // 平坦的迷你边界游戏对象数组
        [HideInInspector]
        public GameObject[] miniBorderFlat;

        // 平坦的小边界游戏对象数组
        [HideInInspector]
        public GameObject[] smallBorderFlat;

        // 平坦的中等边界游戏对象数组
        [HideInInspector]
        public GameObject[] mediumBorderFlat;

        // 平坦的大边界游戏对象数组
        [HideInInspector]
        public GameObject[] largeBorderFlat;

        // 带有城市出口的迷你边界游戏对象数组
        [HideInInspector]
        public GameObject[] miniBorderWithExitOfCity;

        // 带有城市出口的小边界游戏对象数组
        [HideInInspector]
        public GameObject[] smallBorderWithExitOfCity;

        // 带有城市出口的中等边界游戏对象数组
        [HideInInspector]
        public GameObject[] mediumBorderWithExitOfCity;

        // 带有城市出口的大边界游戏对象数组
        [HideInInspector]
        public GameObject[] largeBorderWithExitOfCity;

        // 大区块游戏对象数组
        [HideInInspector]
        public GameObject[] largeBlocks;

        // 用于标记大区块的状态，布尔类型数组
        private bool[] _largeBlocks;

        // 更大的大区块游戏对象数组
        [HideInInspector]
        public GameObject[] bigLargeBlocks;

        // 以下数组用于存储不同方向和距离的游戏对象
        // 向前 50 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forward50;
        // 向前 100 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forward100;
        // 向前 300 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forward300;
        // 向前 400 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forward400;
        // 向前左 400 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forwardLeft400;
        // 向前右 400 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] forwardRight400;
        // 向左 200 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] left200;
        // 向左 300 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] left300;
        // 向右 200 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] right200;
        // 向右 300 单位距离的游戏对象数组
        [HideInInspector]
        public GameObject[] right300;

        // 用于标记更大的大区块的状态，布尔类型数组
        private bool[] _bigLargeBlocks;

        // 以下数组用于存储不同类型的建筑游戏对象
        // 郊区（非角落）的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BB;
        // 市中心（非角落）的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BC;
        // 郊区（非角落）的住宅建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BR;
        // 占据街区两侧的角落建筑游戏对象数组
        [HideInInspector]
        public GameObject[] DC;
        // 郊区的角落建筑游戏对象数组
        [HideInInspector]
        public GameObject[] EB;
        // 市中心的角落建筑游戏对象数组
        [HideInInspector]
        public GameObject[] EC;
        // 占据街区两侧的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] MB;
        // 占据整个街区的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BK;
        // 占据更大街区的大型建筑游戏对象数组
        [HideInInspector]
        public GameObject[] SB;
        // 斜坡上（社区）的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BBS;
        // 市中心斜坡上的建筑游戏对象数组
        [HideInInspector]
        public GameObject[] BCS;

        // 以下数组用于存储对应建筑数组的索引或计数信息
        private int[] _BB;
        private int[] _BC;
        private int[] _BR;
        //private int[] _DC;
        private int[] _EB;
        private int[] _EC;

        private int[] _EBS;
        private int[] _ECS;

        private int[] _MB;
        private int[] _BK;
        private int[] _SB;
        private int[] _BBS;
        private int[] _BCS;

        // 临时数组，用于临时存储游戏对象，具体用途根据代码逻辑而定
        private GameObject[] tempArray;
        // 用于存储建筑数量或索引的整数变量
        private int numB;

        // 存储从城市中心到某个位置的距离，初始值为 300
        float distCenter = 300;
        // 布尔类型变量，用于标记是否存在市中心区域，初始值为 true
        bool withDowntownArea = true;
        // 存储市中心区域的大小，初始值为 100
        float downTownSize = 100;

        // 清除城市的方法，用于销毁当前的城市游戏对象
        public void ClearCity()
        {
            // 如果 cityMaker 为空，则尝试在场景中查找名为 "City-Maker" 的游戏对象
            if (!cityMaker)
                cityMaker = GameObject.Find("City-Maker");

            // 如果找到了 cityMaker 游戏对象，则立即销毁它
            if (cityMaker)
                DestroyImmediate(cityMaker);
        }

        /// <summary>
        /// 生成指定大小的城市，可以选择是否包含卫星城以及边界是否为平坦的
        /// </summary>
        /// <param name="size">城市的大小，1表示非常小的城市，2表示小的城市，3表示中等城市，4表示大城市</param>
        /// <param name="withSatteliteCity">是否包含卫星城，默认为false</param>
        /// <param name="borderFlat">城市边界是否为平坦的，默认为false</param>
        public void GenerateCity(int size, bool withSatteliteCity = false, bool borderFlat = false)
        {
            // 用于标记是否存在卫星城
            bool satCity = false;

            // 根据传入的城市大小参数，调用不同的街道生成方法
            if (size == 1)
            {
                // 非常小的城市
                satCity = GenerateStreetsVerySmall(borderFlat, withSatteliteCity);
            }
            else if (size == 2)
            {
                // 小的城市
                satCity = GenerateStreetsSmall(borderFlat, withSatteliteCity);
            }
            else if (size == 3)
            {
                // 中等城市
                satCity = GenerateStreets(borderFlat, withSatteliteCity);
            }
            else if (size == 4)
            {
                // 大城市
                satCity = GenerateStreetsBig(borderFlat, withSatteliteCity);
            }

            // 如果存在卫星城
            if (satCity)
            {
                // 获取城市出口的位置
                Transform exitPositipon = CityExitPosition();

                // 如果成功获取到城市出口的位置
                if (exitPositipon != null)
                {
                    // 生成一个1到9之间的随机整数
                    int i = (int)Random.Range(1, 10f);

                    // 用于存储实例化的游戏对象
                    GameObject block;

                    // 根据随机数i的值，执行不同的操作
                    switch (i)
                    {
                        case 8:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, 0, -1516);
                            // 在城市出口位置实例化一个forward400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forward400[Random.Range(0, forward400.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方400单位处实例化一个forward400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forward400[Random.Range(0, forward400.Length)], exitPositipon.position + exitPositipon.forward * 400, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方800单位处实例化一个forward400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forward400[Random.Range(0, forward400.Length)], exitPositipon.position + exitPositipon.forward * 800, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 7:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, -300, -1516);
                            // 在城市出口位置实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方400单位且右侧100单位处实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 400 + exitPositipon.right * 100, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方800单位且右侧200单位处实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 800 + exitPositipon.right * 200, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 6:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, 200, -1516);
                            // 在城市出口位置实例化一个forward400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forward400[Random.Range(0, forward400.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方400单位处实例化一个forwardLeft400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], exitPositipon.position + exitPositipon.forward * 400, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方800单位且左侧100单位处实例化一个forwardLeft400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], exitPositipon.position + exitPositipon.forward * 800 - exitPositipon.right * 100, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 5:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, -100, -1516);
                            // 在城市出口位置实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方400单位且右侧100单位处实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 400 + exitPositipon.right * 100, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方800单位且右侧200单位处实例化一个forwardLeft400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardLeft400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 800 + exitPositipon.right * 200, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 4:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, 700, -1316);
                            // 在城市出口位置实例化一个left300数组中的随机游戏对象
                            block = (GameObject)Instantiate(left300[Random.Range(0, left300.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方300单位且左侧300单位处实例化一个right300数组中的随机游戏对象
                            block = (GameObject)Instantiate(right300[Random.Range(0, right300.Length)], exitPositipon.position + exitPositipon.forward * 300 - exitPositipon.right * 300, Quaternion.Euler(0, 270, 0), cityMaker.transform);
                            // 在出口位置前方600单位且左侧600单位处实例化一个forwardLeft400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardLeft400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 600 - exitPositipon.right * 600, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 3:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, 500, -1316);
                            // 在城市出口位置实例化一个left300数组中的随机游戏对象
                            block = (GameObject)Instantiate(left300[Random.Range(0, left300.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方300单位且左侧300单位处实例化一个right300数组中的随机游戏对象
                            block = (GameObject)Instantiate(right300[Random.Range(0, right300.Length)], exitPositipon.position + exitPositipon.forward * 300 - exitPositipon.right * 300, Quaternion.Euler(0, 270, 0), cityMaker.transform);
                            // 在出口位置前方600单位且左侧600单位处实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 600 - exitPositipon.right * 600, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        case 2:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, -700, -1316);
                            // 在城市出口位置实例化一个right300数组中的随机游戏对象
                            block = (GameObject)Instantiate(right300[Random.Range(0, right300.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方300单位且右侧300单位处实例化一个left300数组中的随机游戏对象
                            block = (GameObject)Instantiate(left300[Random.Range(0, left300.Length)], exitPositipon.position + exitPositipon.forward * 300 + exitPositipon.right * 300, Quaternion.Euler(0, 90, 0), cityMaker.transform);
                            // 在出口位置前方600单位且右侧600单位处实例化一个forwardRight400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], exitPositipon.position + exitPositipon.forward * 600 + exitPositipon.right * 600, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;

                        default:
                            // 生成非常小的卫星城街道
                            GenerateStreetsVerySmall(false, false, true, -500, -1316);
                            // 在城市出口位置实例化一个right300数组中的随机游戏对象
                            block = (GameObject)Instantiate(right300[Random.Range(0, right300.Length)], exitPositipon.position, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            // 在出口位置前方300单位且右侧300单位处实例化一个left300数组中的随机游戏对象
                            block = (GameObject)Instantiate(left300[Random.Range(0, left300.Length)], exitPositipon.position + exitPositipon.forward * 300 + exitPositipon.right * 300, Quaternion.Euler(0, 90, 0), cityMaker.transform);
                            // 在出口位置前方600单位且右侧600单位处实例化一个forwardLeft400数组中的随机游戏对象
                            block = (GameObject)Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], exitPositipon.position + exitPositipon.forward * 600 + exitPositipon.right * 600, Quaternion.Euler(0, 0, 0), cityMaker.transform);
                            break;
                    }
                }
                else
                {
                    // 如果未找到出口城市的游戏对象，输出调试信息
                    Debug.Log("ExitCity gameobject not found");
                }
            }

            // 查找场景中的DayNight脚本实例
            DayNight dayNight = FindObjectOfType<DayNight>();
            // 如果找到了DayNight脚本实例，则调用其ChangeMaterial方法
            if (dayNight)
                dayNight.ChangeMaterial();
        }
        /// <summary>
        /// 获取名为 "ExitCity" 的游戏对象的 Transform 组件。
        /// 如果场景中存在名为 "ExitCity" 的游戏对象，则返回其 Transform 组件；
        /// 否则返回 null。
        /// </summary>
        /// <returns>名为 "ExitCity" 的游戏对象的 Transform 组件，若不存在则为 null</returns>
        private Transform CityExitPosition()
        {
            // 检查场景中是否存在名为 "ExitCity" 的游戏对象
            if (GameObject.Find("ExitCity"))
                // 若存在，则返回该游戏对象的 Transform 组件
                return GameObject.Find("ExitCity").transform;
            else
                // 若不存在，则返回 null
                return null;
        }

        /// <summary>
        /// 生成一个非常小的城市街道布局。
        /// </summary>
        /// <param name="borderFlat">是否使用平坦的边界，默认为 false</param>
        /// <param name="withSatteliteCity">是否带有卫星城，默认为 false</param>
        /// <param name="satteliteCity">是否为卫星城，默认为 false</param>
        /// <param name="satteliteCityPositionX">卫星城在 X 轴上的位置，默认为 0</param>
        /// <param name="satteliteCityPositionZ">卫星城在 Z 轴上的位置，默认为 0</param>
        /// <returns>如果带有卫星城且有可用的带出口的迷你边界，则返回 true；否则返回 false</returns>
        private bool GenerateStreetsVerySmall(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false, float satteliteCityPositionX = 0, float satteliteCityPositionZ = 0)
        {
            // 如果是卫星城且城市生成器对象不存在，则将卫星城标志置为 false
            if (satteliteCity && !cityMaker)
                satteliteCity = false;

            // 如果不是卫星城，则清除现有城市并创建一个新的城市生成器对象
            if (!satteliteCity)
            {
                // 调用 ClearCity 方法清除现有城市
                ClearCity();
                // 创建一个名为 "City-Maker" 的新游戏对象作为城市生成器
                cityMaker = new GameObject("City-Maker");
            }

            // 声明一个 GameObject 类型的变量 block，用于存储实例化的游戏对象
            GameObject block;

            // 如果不是卫星城，则将中心距离设置为 150
            if (!satteliteCity)
                distCenter = 150;

            // 声明一个整数变量 nb，用于存储随机选择的索引
            int nb = 0;

            // 获取 largeBlocks 数组的长度
            int le = largeBlocks.Length;
            // 随机生成一个 0 到 le 之间的整数作为索引
            nb = Random.Range(0, le);
            nb = 14;// LLM测试用，指定生成带弯路的街道模板

            // 如果是卫星城且有可用的带出口的小边界
            if (satteliteCity && smallBorderWithExitOfCity.Length > 0)
                // 在卫星城指定位置实例化一个 largeBlocks 数组中的随机元素
                block = (GameObject)Instantiate(largeBlocks[nb], CityExitPosition().position + new Vector3(satteliteCityPositionX, 0, satteliteCityPositionZ) - new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            else
                // 在原点位置实例化一个 largeBlocks 数组中的随机元素
                block = (GameObject)Instantiate(largeBlocks[nb], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

            // 如果带有卫星城或者是卫星城，并且有可用的带出口的迷你边界
            if ((withSatteliteCity || satteliteCity) && miniBorderWithExitOfCity.Length > 0)
            {
                if (satteliteCity)
                    // 在卫星城指定位置实例化一个带出口的迷你边界的随机元素，并旋转 180 度
                    block = (GameObject)Instantiate(miniBorderWithExitOfCity[Random.Range(0, miniBorderWithExitOfCity.Length)], CityExitPosition().position + new Vector3(satteliteCityPositionX, 0, satteliteCityPositionZ), Quaternion.Euler(0, 180, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个带出口的迷你边界的随机元素
                    block = (GameObject)Instantiate(miniBorderWithExitOfCity[Random.Range(0, miniBorderWithExitOfCity.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            }
            else
            {
                if (borderFlat)
                    // 如果使用平坦边界，在原点位置实例化一个平坦的迷你边界的随机元素
                    block = (GameObject)Instantiate(miniBorderFlat[Random.Range(0, miniBorderFlat.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个普通的迷你边界的随机元素
                    block = (GameObject)Instantiate(miniBorder[Random.Range(0, miniBorder.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            }

            // 将实例化的游戏对象设置为城市生成器对象的子对象
            block.transform.SetParent(cityMaker.transform);

            // 如果带有卫星城且有可用的带出口的迷你边界，则返回 true；否则返回 false
            return (withSatteliteCity && miniBorderWithExitOfCity.Length > 0);
        }
        /// <summary>
        /// 生成一个小型城市的街道布局。
        /// </summary>
        /// <param name="borderFlat">是否使用平坦的边界，默认为 false</param>
        /// <param name="withSatteliteCity">是否带有卫星城，默认为 false</param>
        /// <param name="satteliteCity">是否为卫星城，默认为 false</param>
        /// <returns>如果带有卫星城且有可用的带出口的小边界，则返回 true；否则返回 false</returns>
        private bool GenerateStreetsSmall(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
        {
            // 如果是卫星城且城市生成器对象不存在，则将卫星城标志置为 false
            if (satteliteCity && !cityMaker)
                satteliteCity = false;

            // 如果不是卫星城，则清除现有城市并创建一个新的城市生成器对象
            if (!satteliteCity)
            {
                // 调用 ClearCity 方法清除现有城市
                ClearCity();

                // 创建一个名为 "City-Maker" 的新游戏对象作为城市生成器
                cityMaker = new GameObject("City-Maker");

            }

            // 如果不是卫星城，则将中心距离设置为 200
            if (!satteliteCity)
                distCenter = 200;

            // 声明一个整数变量 nb，用于存储随机选择的索引
            int nb = 0;

            // 获取 largeBlocks 数组的长度
            int le = largeBlocks.Length;
            // 初始化一个布尔数组，用于标记 largeBlocks 数组中的元素是否已被使用
            _largeBlocks = new bool[largeBlocks.Length];

            // 定义一个长度为 3 的 Vector3 数组，用于存储大区块的位置
            Vector3[] ps = new Vector3[3];

            // 定义一个长度为 3 的整数数组，用于存储大区块的旋转角度
            int[] rt = new int[3];

            // 生成一个 0 到 6 之间的随机浮点数
            float s = Random.Range(0, 6f);

            // 根据随机数 s 的值来决定大区块的位置和旋转角度
            if (s < 3)
            {
                // 第一个大区块的位置为原点，旋转角度为 0
                ps[1] = new Vector3(0, 0, 0); rt[1] = 0;
                // 第二个大区块的位置为 Z 轴正方向 300 单位处，旋转角度为 0
                ps[2] = new Vector3(0, 0, 300); rt[2] = 0;
            }
            else
            {
                // 第一个大区块的位置为 (-150, 0, 150)，旋转角度为 90 度
                ps[1] = new Vector3(-150, 0, 150); rt[1] = 90;
                // 第二个大区块的位置为 (150, 0, 150)，旋转角度为 90 度
                ps[2] = new Vector3(150, 0, 150); rt[2] = 90;
            }

            // 循环实例化 2 个大区块
            for (int qt = 1; qt < 3; qt++)
            {
                // 尝试 100 次，随机选择一个未被使用的大区块
                for (int lp = 0; lp < 100; lp++)
                {
                    nb = Random.Range(0, le);
                    // 如果该大区块未被使用，则跳出循环
                    if (!_largeBlocks[nb]) break;
                }
                // 标记该大区块已被使用
                _largeBlocks[nb] = true;

                // 如果是卫星城且有可用的带出口的小边界
                if (satteliteCity && smallBorderWithExitOfCity.Length > 0)
                    // 在指定位置实例化大区块，并旋转 180 度
                    Instantiate(largeBlocks[nb], ps[qt] + CityExitPosition().position + new Vector3(-0, 0, -1516) - new Vector3(0, 0, 300), Quaternion.Euler(0, rt[qt] + 180, 0), cityMaker.transform);
                else
                    // 在指定位置实例化大区块
                    Instantiate(largeBlocks[nb], ps[qt], Quaternion.Euler(0, rt[qt], 0), cityMaker.transform);

            }

            // 声明一个 GameObject 类型的变量 block，用于存储实例化的游戏对象
            GameObject block;

            // 如果带有卫星城或者是卫星城，并且有可用的带出口的小边界
            if ((withSatteliteCity || satteliteCity) && smallBorderWithExitOfCity.Length > 0)
            {
                if (satteliteCity)
                    // 在卫星城出口位置实例化一个带出口的小边界的随机元素，并旋转 180 度
                    block = (GameObject)Instantiate(smallBorderWithExitOfCity[Random.Range(0, smallBorderWithExitOfCity.Length)], CityExitPosition().position + new Vector3(-0, 0, -1516), Quaternion.Euler(0, 180, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个带出口的小边界的随机元素
                    block = (GameObject)Instantiate(smallBorderWithExitOfCity[Random.Range(0, smallBorderWithExitOfCity.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);

            }
            else
            {
                if (borderFlat)
                    // 如果使用平坦边界，在原点位置实例化一个平坦的小边界的随机元素
                    block = (GameObject)Instantiate(smallBorderFlat[Random.Range(0, smallBorderFlat.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个普通的小边界的随机元素
                    block = (GameObject)Instantiate(smallBorder[Random.Range(0, smallBorder.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            }

            // 将实例化的游戏对象设置为城市生成器对象的子对象
            block.transform.SetParent(cityMaker.transform);

            // 如果带有卫星城且有可用的带出口的小边界，则返回 true；否则返回 false
            return (withSatteliteCity && smallBorderWithExitOfCity.Length > 0);

        }


        /// <summary>
        /// 生成中等规模城市的街道布局。
        /// </summary>
        /// <param name="borderFlat">是否使用平坦的边界，默认为 false</param>
        /// <param name="withSatteliteCity">是否带有卫星城，默认为 false</param>
        /// <param name="satteliteCity">是否为卫星城，默认为 false</param>
        /// <returns>如果带有卫星城且有可用的带出口的中等边界，则返回 true；否则返回 false</returns>
        private bool GenerateStreets(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
        {
            // 如果是卫星城且城市生成器对象不存在，则将卫星城标志置为 false
            if (satteliteCity && !cityMaker)
                satteliteCity = false;

            // 如果不是卫星城，则清除现有城市并创建一个新的城市生成器对象
            if (!satteliteCity)
            {
                // 调用 ClearCity 方法清除现有城市
                ClearCity();

                // 创建一个名为 "City-Maker" 的新游戏对象作为城市生成器
                cityMaker = new GameObject("City-Maker");
            }

            // 如果不是卫星城，则将中心距离设置为 300
            if (!satteliteCity)
                distCenter = 300;

            // 声明一个整数变量 nb，用于存储随机选择的索引
            int nb = 0;

            // 获取 largeBlocks 数组的长度
            int le = largeBlocks.Length;
            // 初始化一个布尔数组，用于标记 largeBlocks 数组中的元素是否已被使用
            _largeBlocks = new bool[largeBlocks.Length];

            // 定义一个长度为 5 的 Vector3 数组，用于存储大区块的位置
            Vector3[] ps = new Vector3[5];

            // 定义一个长度为 5 的整数数组，用于存储大区块的旋转角度
            int[] rt = new int[5];

            // 生成一个 0 到 6 之间的随机浮点数
            float s = Random.Range(0, 6f);

            // 根据随机数 s 的值来决定大区块的位置和旋转角度
            if (s < 2)
            {
                // 第一个大区块的位置为原点，旋转角度为 0
                ps[1] = new Vector3(0, 0, 0); rt[1] = 0;
                // 第二个大区块的位置为 Z 轴正方向 300 单位处，旋转角度为 0
                ps[2] = new Vector3(0, 0, 300); rt[2] = 0;
                // 第三个大区块的位置为 (450, 0, 150)，旋转角度为 90 度
                ps[3] = new Vector3(450, 0, 150); rt[3] = 90;
                // 第四个大区块的位置为 (-450, 0, 150)，旋转角度为 90 度
                ps[4] = new Vector3(-450, 0, 150); rt[4] = 90;

            }
            else if (s < 3)
            {
                // 第一个大区块的位置为 (-450, 0, 150)，旋转角度为 90 度
                ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
                // 第二个大区块的位置为 (-150, 0, 150)，旋转角度为 90 度
                ps[2] = new Vector3(-150, 0, 150); rt[2] = 90;
                // 第三个大区块的位置为 (150, 0, 150)，旋转角度为 90 度
                ps[3] = new Vector3(150, 0, 150); rt[3] = 90;
                // 第四个大区块的位置为 (450, 0, 150)，旋转角度为 90 度
                ps[4] = new Vector3(450, 0, 150); rt[4] = 90;

            }
            else if (s < 4)
            {
                // 第一个大区块的位置为 (-450, 0, 150)，旋转角度为 90 度
                ps[1] = new Vector3(-450, 0, 150); rt[1] = 90;
                // 第二个大区块的位置为 (-150, 0, 150)，旋转角度为 90 度
                ps[2] = new Vector3(-150, 0, 150); rt[2] = 90;
                // 第三个大区块的位置为 (300, 0, 0)，旋转角度为 0
                ps[3] = new Vector3(300, 0, 0); rt[3] = 0;
                // 第四个大区块的位置为 (300, 0, 300)，旋转角度为 0
                ps[4] = new Vector3(300, 0, 300); rt[4] = 0;

            }
            else
            {
                // 第一个大区块的位置为 (450, 0, 150)，旋转角度为 90 度
                ps[1] = new Vector3(450, 0, 150); rt[1] = 90;
                // 第二个大区块的位置为 (150, 0, 150)，旋转角度为 90 度
                ps[2] = new Vector3(150, 0, 150); rt[2] = 90;
                // 第三个大区块的位置为 (-300, 0, 0)，旋转角度为 0
                ps[3] = new Vector3(-300, 0, 0); rt[3] = 0;
                // 第四个大区块的位置为 (-300, 0, 300)，旋转角度为 0
                ps[4] = new Vector3(-300, 0, 300); rt[4] = 0;

            }

            // 循环实例化 4 个大区块
            for (int qt = 1; qt < 5; qt++)
            {
                // 尝试 100 次，随机选择一个未被使用的大区块
                for (int lp = 0; lp < 100; lp++)
                {
                    nb = Random.Range(0, le);
                    // 如果该大区块未被使用，则跳出循环
                    if (!_largeBlocks[nb]) break;
                }
                // 标记该大区块已被使用
                _largeBlocks[nb] = true;

                // 在指定位置和旋转角度实例化大区块，并将其作为城市生成器对象的子对象
                Instantiate(largeBlocks[nb], ps[qt], Quaternion.Euler(0, rt[qt], 0), cityMaker.transform);
            }

            // 声明一个 GameObject 类型的变量 block，用于存储实例化的游戏对象
            GameObject block;

            // 如果带有卫星城或者是卫星城，并且有可用的带出口的中等边界
            if ((withSatteliteCity || satteliteCity) && mediumBorderWithExitOfCity.Length > 0)
                // 在原点位置实例化一个带出口的中等边界的随机元素
                block = (GameObject)Instantiate(mediumBorderWithExitOfCity[Random.Range(0, mediumBorderWithExitOfCity.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            else
            {
                if (borderFlat)
                    // 如果使用平坦边界，在原点位置实例化一个平坦的中等边界的随机元素
                    block = (GameObject)Instantiate(mediumBorderFlat[Random.Range(0, mediumBorderFlat.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个普通的中等边界的随机元素
                    block = (GameObject)Instantiate(mediumBorder[Random.Range(0, mediumBorder.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            }

            // 将实例化的游戏对象设置为城市生成器对象的子对象
            block.transform.SetParent(cityMaker.transform);

            // 如果带有卫星城且有可用的带出口的中等边界，则返回 true；否则返回 false
            return (withSatteliteCity && mediumBorderWithExitOfCity.Length > 0);
        }

        /// <summary>
        /// 生成大型城市的街道布局。
        /// </summary>
        /// <param name="borderFlat">是否使用平坦的边界，默认为 false</param>
        /// <param name="withSatteliteCity">是否带有卫星城，默认为 false</param>
        /// <param name="satteliteCity">是否为卫星城，默认为 false</param>
        /// <returns>如果带有卫星城且有可用的带出口的大型边界，则返回 true；否则返回 false</returns>
        private bool GenerateStreetsBig(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
        {
            // 如果是卫星城且城市生成器对象不存在，则将卫星城标志置为 false
            if (satteliteCity && !cityMaker)
                satteliteCity = false;

            // 如果不是卫星城，则清除现有城市并创建一个新的城市生成器对象
            if (!satteliteCity)
            {
                // 调用 ClearCity 方法清除现有城市
                ClearCity();

                // 创建一个名为 "City-Maker" 的新游戏对象作为城市生成器
                cityMaker = new GameObject("City-Maker");
            }

            // 设置城市中心距离为 350
            distCenter = 350;

            // 声明一个整数变量 nb，用于存储随机选择的索引
            int nb = 0;

            // 获取 largeBlocks 数组的长度
            int le = largeBlocks.Length;
            // 获取 bigLargeBlocks 数组的长度
            int lebig = bigLargeBlocks.Length;

            // 初始化一个布尔数组，用于标记 largeBlocks 数组中的元素是否已被使用
            _largeBlocks = new bool[largeBlocks.Length];
            // 初始化一个布尔数组，用于标记 bigLargeBlocks 数组中的元素是否已被使用
            _bigLargeBlocks = new bool[bigLargeBlocks.Length];

            // 定义一个长度为 7 的 Vector3 数组，用于存储街区的位置
            Vector3[] ps = new Vector3[7];

            // 定义一个长度为 7 的整数数组，用于存储街区的旋转角度
            int[] rt = new int[7];

            // 定义一个长度为 7 的整数数组，用于存储街区的类型（1 表示大型街区，2 表示超大型街区）
            int[] tb = new int[7];

            // 声明一个整数变量 qt，用于存储需要实例化的街区数量
            int qt;

            // 生成一个 0 到 7 之间的随机浮点数
            float s = Random.Range(0, 7f);

            // 根据随机数 s 的值来决定街区的位置、旋转角度和类型
            if (s < 3)
            {
                // 需要实例化的街区数量为 6
                qt = 6;

                // 第一个街区的位置为原点，旋转角度为 0，类型为大型街区
                ps[1] = new Vector3(0, 0, 0); rt[1] = 0; tb[1] = 1;
                // 第二个街区的位置为 Z 轴正方向 300 单位处，旋转角度为 0，类型为大型街区
                ps[2] = new Vector3(0, 0, 300); rt[2] = 0; tb[2] = 1;
                // 第三个街区的位置为 (450, 0, 150)，旋转角度为 90 度，类型为大型街区
                ps[3] = new Vector3(450, 0, 150); rt[3] = 90; tb[3] = 1;
                // 第四个街区的位置为 (-450, 0, 150)，旋转角度为 90 度，类型为大型街区
                ps[4] = new Vector3(-450, 0, 150); rt[4] = 90; tb[4] = 1;
                // 第五个街区的位置为 (-300, 0, 600)，旋转角度为 0，类型为大型街区
                ps[5] = new Vector3(-300, 0, 600); rt[5] = 0; tb[5] = 1;
                // 第六个街区的位置为 (300, 0, 600)，旋转角度为 0，类型为大型街区
                ps[6] = new Vector3(300, 0, 600); rt[6] = 0; tb[6] = 1;

            }
            // 此条件与上一个 if 条件重复，可能是代码编写错误
            else if (s < 3)
            {
                qt = 6;
                ps[1] = new Vector3(-450, 0, 150); rt[1] = 90; tb[1] = 1;
                ps[2] = new Vector3(-150, 0, 150); rt[2] = 90; tb[2] = 1;
                ps[3] = new Vector3(150, 0, 150); rt[3] = 90; tb[3] = 1;
                ps[4] = new Vector3(450, 0, 150); rt[4] = 90; tb[4] = 1;
                ps[5] = new Vector3(-300, 0, 600); rt[5] = 0; tb[5] = 1;
                ps[6] = new Vector3(300, 0, 600); rt[6] = 0; tb[6] = 1;

            }
            else if (s < 4)
            {
                qt = 6;
                ps[1] = new Vector3(-300, 0, 300); rt[1] = 0; tb[1] = 1;
                ps[2] = new Vector3(-300, 0, 0); rt[2] = 0; tb[2] = 1;
                ps[3] = new Vector3(150, 0, 150); rt[3] = 90; tb[3] = 1;
                ps[4] = new Vector3(450, 0, 150); rt[4] = 90; tb[4] = 1;
                ps[5] = new Vector3(-300, 0, 600); rt[5] = 0; tb[5] = 1;
                ps[6] = new Vector3(300, 0, 600); rt[6] = 0; tb[6] = 1;

            }
            else if (s < 5)
            {
                qt = 5;
                ps[1] = new Vector3(-300, 0, 0); rt[1] = 0; tb[1] = 1;
                ps[2] = new Vector3(300, 0, 0); rt[2] = 0; tb[2] = 1;
                ps[3] = new Vector3(-300, 0, 600); rt[3] = 0; tb[3] = 1;
                ps[4] = new Vector3(300, 0, 600); rt[4] = 0; tb[4] = 1;
                ps[5] = new Vector3(0, 0, 300); rt[5] = 0; tb[5] = 2;

            }
            else
            {
                qt = 6;
                ps[1] = new Vector3(-450, 0, 150); rt[1] = 90; tb[1] = 1;
                ps[2] = new Vector3(300, 0, 0); rt[2] = 0; tb[2] = 1;
                ps[3] = new Vector3(-150, 0, 150); rt[3] = 90; tb[3] = 1;
                ps[4] = new Vector3(450, 0, 450); rt[4] = 90; tb[4] = 1;
                ps[5] = new Vector3(-300, 0, 600); rt[5] = 0; tb[5] = 1;
                ps[6] = new Vector3(150, 0, 450); rt[6] = 90; tb[6] = 1;

            }

            // 循环实例化指定数量的街区
            for (int count = 1; count <= qt; count++)
            {
                // 如果街区类型为大型街区
                if (tb[count] == 1)
                {
                    // 尝试 100 次，随机选择一个未被使用的大型街区
                    for (int lp = 0; lp < 100; lp++)
                    {
                        nb = Random.Range(0, le);
                        // 如果该大型街区未被使用，则跳出循环
                        if (!_largeBlocks[nb]) break;
                    }
                    // 标记该大型街区已被使用
                    _largeBlocks[nb] = true;

                    // 在指定位置和旋转角度实例化大型街区，并将其作为城市生成器对象的子对象
                    Instantiate(largeBlocks[nb], ps[count], Quaternion.Euler(0, rt[count], 0), cityMaker.transform);
                }
                // 如果街区类型为超大型街区
                else if (tb[count] == 2)
                {
                    // 尝试 100 次，随机选择一个未被使用的超大型街区
                    for (int lp = 0; lp < 100; lp++)
                    {
                        nb = Random.Range(0, lebig);
                        // 如果该超大型街区未被使用，则跳出循环
                        if (!_bigLargeBlocks[nb]) break;
                    }
                    // 标记该超大型街区已被使用
                    _bigLargeBlocks[nb] = true;

                    // 在指定位置和旋转角度实例化超大型街区，并将其作为城市生成器对象的子对象
                    Instantiate(bigLargeBlocks[nb], ps[count], Quaternion.Euler(0, rt[count], 0), cityMaker.transform);
                }
            }

            // 声明一个 GameObject 类型的变量 block，用于存储实例化的游戏对象
            GameObject block;

            // 如果带有卫星城或者是卫星城，并且有可用的带出口的大型边界
            if ((withSatteliteCity || satteliteCity) && largeBorderWithExitOfCity.Length > 0)
                // 在原点位置实例化一个带出口的大型边界的随机元素
                block = (GameObject)Instantiate(largeBorderWithExitOfCity[Random.Range(0, largeBorderWithExitOfCity.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            else
            {
                if (borderFlat)
                    // 如果使用平坦边界，在原点位置实例化一个平坦的大型边界的随机元素
                    block = (GameObject)Instantiate(largeBorderFlat[Random.Range(0, largeBorderFlat.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
                else
                    // 在原点位置实例化一个普通的大型边界的随机元素
                    block = (GameObject)Instantiate(largeBorder[Random.Range(0, largeBorder.Length)], new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), cityMaker.transform);
            }

            // 将实例化的游戏对象设置为城市生成器对象的子对象
            block.transform.SetParent(cityMaker.transform);

            // 如果带有卫星城且有可用的带出口的大型边界，则返回 true；否则返回 false
            return (withSatteliteCity && largeBorderWithExitOfCity.Length > 0);
        }
        // 私有变量，用于存储一个临时的 GameObject
        private GameObject pB;

        // 公共方法，用于生成所有建筑物
        // _withDowntownArea: 是否包含市中心区域的布尔值
        // _downTownSize: 市中心区域的大小
        public void GenerateAllBuildings(bool _withDowntownArea, float _downTownSize)
        {
            // 将传入的市中心区域大小赋值给类的成员变量
            downTownSize = _downTownSize;
            // 将传入的是否包含市中心区域的标志赋值给类的成员变量
            withDowntownArea = _withDowntownArea;

            // 如果需要生成市中心区域
            if (withDowntownArea)
            {
                // 查找场景中所有名称为 "Marcador" 的 GameObject
                GameObject[] tArray = GameObject.FindObjectsOfType(typeof(GameObject))
                    .Select(g => g as GameObject)
                    .Where(g => g.name == ("Marcador"))
                    .ToArray();

                // 如果只找到一个 "Marcador" GameObject
                if (tArray.Length == 1)
                {
                    // 将该 GameObject 的位置作为城市中心位置
                    center = tArray[0].transform.position;
                }
                else
                {
                    // 随机选择一个 "Marcador" GameObject 的位置作为城市中心位置
                    center = tArray[Random.Range(1, tArray.Length - 1)].transform.position;
                }

                // 如果场景中存在名为 "DownTownPosition" 的 GameObject，并且随机数小于 5
                if (GameObject.Find("DownTownPosition") && Random.Range(1, 10) < 5)
                {
                    // 将 "DownTownPosition" 的位置作为城市中心位置
                    center = GameObject.Find("DownTownPosition").transform.position;
                }
            }

            // 初始化各种建筑物类型的计数器数组
            _BB = new int[BB.Length];
            _BC = new int[BC.Length];
            _BR = new int[BR.Length];
            //_DC = new int[DC.Length];
            _EB = new int[EB.Length];
            _EC = new int[EC.Length];
            _MB = new int[MB.Length];
            _BK = new int[BK.Length];
            _SB = new int[SB.Length];

            _EBS = new int[EB.Length];
            _ECS = new int[EC.Length];

            _BBS = new int[BBS.Length];
            _BCS = new int[BCS.Length];

            // 初始化住宅建筑物计数器为 0
            residential = 0;

            // 销毁现有的建筑物
            DestroyBuildings();

            // 创建一个临时的 GameObject，用于后续操作
            pB = new GameObject();

            // 初始化建筑物数量计数器为 0
            nB = 0;

            // 在超级街区中创建建筑物
            CreateBuildingsInSuperBlocks();
            // 在普通街区中创建建筑物
            CreateBuildingsInBlocks();
            // 在线条区域创建建筑物
            CreateBuildingsInLines();
            // 在双区域创建建筑物
            CreateBuildingsInDouble();

            // 清空开发者控制台的日志
            Debug.ClearDeveloperConsole();
            // 输出创建的建筑物数量到控制台
            Debug.Log(nB + " buildings were created");

            //// 销毁临时的 GameObject（原代码）
            //if (pB != null && PrefabUtility.IsPartOfPrefabInstance(pB))
            //{
            //    DestroyImmediate(pB, true);
            //}
            // 销毁临时的 GameObject
            if (pB != null)
            {
                DestroyImmediate(pB, true);
            }

            // 查找场景中的 DayNight 组件
            DayNight dayNight = FindObjectOfType<DayNight>();
            // 如果找到了 DayNight 组件
            if (dayNight)
            {
                // 调用 DayNight 组件的 ChangeMaterial 方法
                dayNight.ChangeMaterial();
            }
        }



        // 公共方法，用于在线条区域创建建筑物
        public void CreateBuildingsInLines()
        {
            // 查找场景中所有名称为 "Marcador" 的 GameObject，并将其存储在 tempArray 数组中
            tempArray = GameObject.FindObjectsOfType(typeof(GameObject))
                .Select(g => g as GameObject)
                .Where(g => g.name == ("Marcador"))
                .ToArray();

            // 遍历所有找到的 "Marcador" GameObject
            foreach (GameObject lines in tempArray)
            {
                // 判断当前区域是否为住宅区
                // 条件：住宅数量小于 15 且当前 "Marcador" 与城市中心的距离大于 400 且随机数小于 30
                _residential = (residential < 15 && Vector3.Distance(center, lines.transform.position) > 400 && Random.Range(0, 100) < 30);

                // 遍历当前 "Marcador" GameObject 的所有子对象
                foreach (Transform child in lines.transform)
                {
                    // 如果子对象的名称为 "E"
                    if (child.name == "E")
                    {
                        // 调用 CreateBuildingsInCorners 方法在角落创建建筑物
                        CreateBuildingsInCorners(child.gameObject);
                    }
                    // 如果子对象的名称为 "EL"
                    else if (child.name == "EL")
                    {
                        // 初始化尝试次数计数器
                        int ct = 0;
                        // 最多尝试 300 次
                        do
                        {
                            // 尝试次数加 1
                            ct++;
                            // 调用 CreateBuildingsInCorners 方法在角落创建建筑物，并传入 true 参数
                            // 如果创建成功，则跳出循环
                            if (CreateBuildingsInCorners(child.gameObject, true))
                                break;
                        } while (ct < 300);
                    }
                    // 如果子对象的名称以 "S" 开头
                    else if (child.name.Substring(0, 1) == "S")
                    {
                        // 调用 CreateBuildingsInLine 方法在线条区域创建建筑物，并传入 90 度旋转角度和 true 参数
                        CreateBuildingsInLine(child.gameObject, 90f, true);
                    }
                    else
                    {
                        // 调用 CreateBuildingsInLine 方法在线条区域创建建筑物，并传入 90 度旋转角度
                        CreateBuildingsInLine(child.gameObject, 90f);
                    }
                }

                // 重置住宅区标志为 false
                _residential = false;
            }
        }
        // 公共方法，用于在角落创建建筑物
        // child: 父 GameObject，建筑物将作为其子对象创建
        // notAnyone: 布尔值，用于控制是否对建筑物进行额外的筛选
        public bool CreateBuildingsInCorners(GameObject child, bool notAnyone = false)
        {
            // 用于存储实例化的建筑物
            GameObject pBuilding;

            // 初始化临时建筑物引用为 null
            pB = null;
            // 用于存储随机选择的建筑物索引
            int numB = 0;
            // 循环计数器，限制尝试次数
            int t = 0;
            // 建筑物的宽度
            float pWidth = 0;
            // 建筑物的长度
            float wComprimento;

            // 建筑物的缩放比例
            float pScale;
            // 剩余可用空间的长度
            float remainingMeters;
            // 用于创建新的标记 GameObject
            GameObject newMarcador;

            // 计算子对象与城市中心的距离
            float distancia = Vector3.Distance(center, child.transform.position);

            // 循环计数器，用于选择合适的建筑物
            int lp;
            lp = 0;
            // 内部循环计数器，用于随机选择建筑物索引
            int lt = 0;

            // 根据市中心大小调整距离阈值
            float _distCenter = distCenter * (Mathf.Clamp(downTownSize, 50, 200) / 100);

            // 最多尝试 100 次选择合适的建筑物
            while (t < 100)
            {
                t++;

                // 如果子对象在市中心区域内
                if (distancia < _distCenter && withDowntownArea)
                {
                    // 尝试选择合适的市中心角落建筑物
                    do
                    {
                        lp++;
                        lt = 0;
                        // 最多尝试 2000 次随机选择建筑物索引
                        do
                        {
                            lt++;
                            // 随机选择一个市中心角落建筑物的索引
                            numB = Random.Range(0, EC.Length);
                        } while (notAnyone && _ECS[numB] > 0 && lt < 2000);

                        // 如果该建筑物的使用次数为 0，跳出循环
                        if (_EC[numB] == 0) break;
                        // 根据尝试次数和使用次数的条件跳出循环
                        if (lp > 100 && _EC[numB] <= 1) break;
                        if (lp > 150 && _EC[numB] <= 2) break;
                        if (lp > 200 && _EC[numB] <= 3) break;
                        if (lp > 250) break;

                    } while (lp < 300);

                    // 获取所选建筑物的宽度
                    pWidth = GetWith(EC[numB]);

                    // 如果宽度小于等于 0，记录错误信息并标记该建筑物不可用
                    if (pWidth <= 0)
                    {
                        Debug.LogWarning("Error: EC: " + numB);
                        _EC[numB] = 100;
                        return false;
                    }
                    // 如果宽度小于等于 36.3f，选择该建筑物并增加使用次数
                    else if (pWidth <= 36.3f)
                    {
                        _EC[numB] += 1;
                        pB = EC[numB];
                        break;
                    }
                }
                else
                {
                    // 尝试选择合适的郊区角落建筑物
                    do
                    {
                        lp++;
                        // 最多尝试 2000 次随机选择建筑物索引
                        do
                        {
                            lt++;
                            // 随机选择一个郊区角落建筑物的索引
                            numB = Random.Range(0, EB.Length);
                        } while (notAnyone && _EBS[numB] >= 100 && lt < 2000);

                        // 如果该建筑物的使用次数为 0，跳出循环
                        if (_EB[numB] == 0) break;
                        // 根据尝试次数和使用次数的条件跳出循环
                        if (lp > 100 && _EB[numB] <= 1) break;
                        if (lp > 150 && _EB[numB] <= 2) break;
                        if (lp > 200 && _EB[numB] <= 3) break;
                        if (lp > 250) break;

                    } while (lp < 300);

                    // 获取所选建筑物的宽度
                    pWidth = GetWith(EB[numB]);

                    // 如果宽度小于等于 0，记录错误信息并标记该建筑物不可用
                    if (pWidth <= 0)
                    {
                        Debug.LogWarning("Error: EB: " + numB);
                        _EB[numB] = 100;
                        return false;
                    }
                    // 如果宽度小于等于 36.3f，选择该建筑物并增加使用次数
                    else if (pWidth <= 36.3f)
                    {
                        _EB[numB] += 1;
                        pB = EB[numB];
                        break;
                    }
                }
            }

            // 实例化所选的建筑物
            pBuilding = (GameObject)Instantiate(pB, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));

            // 如果需要额外筛选且建筑物不满足斜坡条件
            if (notAnyone && !TestBaseBuildindCornerOnTheSlope(pBuilding.transform))
            {
                // 根据建筑物所在区域，调整使用次数和标记
                if (distancia < _distCenter && withDowntownArea)
                {
                    _ECS[numB] = 100;
                    _EC[numB] -= 1;
                }
                else
                {
                    _EBS[numB] = 100;
                    _EB[numB] -= 1;
                }

                // 销毁实例化的建筑物
                DestroyImmediate(pBuilding);

                return false;
            }

            // 设置建筑物的名称
            pBuilding.name = pBuilding.name;
            // 将建筑物设置为子对象
            pBuilding.transform.SetParent(child.transform);
            // 设置建筑物的本地位置
            pBuilding.transform.localPosition = new Vector3(-(pWidth * 0.5f), 0, 0);
            // 设置建筑物的本地旋转
            pBuilding.transform.localRotation = Quaternion.Euler(0, 0, 0);

            // 增加建筑物数量计数器
            nB++;

            // 检查角落建筑物后面的空间
            wComprimento = GetHeight(pB);
            if (wComprimento < 29.9f)
            {
                // 创建一个新的标记 GameObject
                newMarcador = new GameObject("Marcador");

                // 将标记 GameObject 设置为子对象
                newMarcador.transform.SetParent(child.transform);
                // 设置标记 GameObject 的本地位置
                newMarcador.transform.localPosition = new Vector3(0, 0, -36);
                // 设置标记 GameObject 的本地旋转
                newMarcador.transform.localRotation = Quaternion.Euler(0, 0, 0);
                // 设置标记 GameObject 的名称为剩余空间长度
                newMarcador.name = (36 - wComprimento).ToString();
                // 在标记位置创建线条建筑物
                CreateBuildingsInLine(newMarcador, 90);
            }
            else
            {
                // 计算剩余空间长度
                remainingMeters = 36 - wComprimento;
                // 计算缩放比例
                pScale = 1 + (remainingMeters / wComprimento);
                // 缩放建筑物
                pBuilding.transform.localScale = new Vector3(1, 1, pScale);
            }

            // 检查角落建筑物侧面的空间
            if (pWidth < 29.9f)
            {
                // 创建一个新的标记 GameObject
                newMarcador = new GameObject("Marcador");

                // 将标记 GameObject 设置为子对象
                newMarcador.transform.SetParent(child.transform);
                // 设置标记 GameObject 的本地位置
                newMarcador.transform.localPosition = new Vector3(-pWidth, 0, 0);
                // 设置标记 GameObject 的本地旋转
                newMarcador.transform.localRotation = Quaternion.Euler(0, 270, 0);
                // 设置标记 GameObject 的名称为剩余空间长度
                newMarcador.name = (36 - pWidth).ToString();
                // 在标记位置创建线条建筑物
                CreateBuildingsInLine(newMarcador, 90);
            }
            else
            {
                // 计算剩余空间长度
                remainingMeters = 36 - pWidth;
                // 计算缩放比例
                pScale = 1 + (remainingMeters / pWidth);
                // 缩放建筑物
                pBuilding.transform.localScale = new Vector3(pScale, 1, 1);
            }

            return true;
        }
        // 检查角落建筑的基础是否在斜坡上
        // buildingCornerOnTheSlope: 待检查的角落建筑的 Transform
        // 返回值: 如果建筑基础有指定的碰撞器，则返回 true，否则返回 false
        bool TestBaseBuildindCornerOnTheSlope(Transform buildingCornerOnTheSlope)
        {
            // 检查建筑的 Transform 下是否存在指定名称的碰撞器
            // 如果存在任何一个指定名称的碰撞器，则返回 true
            return (buildingCornerOnTheSlope.Find("Base-Corner-0-Collider") || buildingCornerOnTheSlope.Find("Base-Corner-03-Collider") || buildingCornerOnTheSlope.Find("Base-Corner-06-Collider"));
        }

        // 生成随机的旋转角度
        // 返回值: 随机生成的旋转角度（0、90、180 或 270）
        int RandRotation()
        {
            // 初始化旋转角度为 0
            int r = 0;
            // 随机生成一个 0 到 3 之间的整数
            int i = Random.Range(0, 4);
            // 根据随机数设置旋转角度
            if (i == 3) r = 180;
            else if (i == 2) r = 90;
            else if (i == 1) r = 270;
            else r = 0;

            return r;
        }

        // 在街区中创建建筑
        public void CreateBuildingsInBlocks()
        {
            // 用于存储随机选择的建筑索引
            int numB = 0;

            // 查找场景中所有名称为 "Blocks" 的 GameObject
            tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Blocks")).ToArray();

            // 遍历所有找到的 "Blocks" GameObject
            foreach (GameObject bks in tempArray)
            {
                // 遍历 "Blocks" GameObject 下的所有子对象
                foreach (Transform bk in bks.transform)
                {
                    // 以一定概率决定是否创建单个大型建筑
                    if (Random.Range(0, 20) > 5)
                    {
                        // 用于记录尝试次数
                        int lp = 0;
                        do
                        {
                            lp++;
                            // 随机选择一个大型建筑的索引
                            numB = Random.Range(0, BK.Length);
                            // 如果该建筑的使用次数为 0，跳出循环
                            if (_BK[numB] == 0) break;
                            // 根据尝试次数和使用次数的条件跳出循环
                            if (lp > 125 && _BK[numB] <= 1) break;
                            if (lp > 150 && _BK[numB] <= 2) break;
                            if (lp > 200 && _BK[numB] <= 3) break;
                            if (lp > 250) break;
                        } while (lp < 300);

                        // 增加该建筑的使用次数
                        _BK[numB] += 1;

                        // 在当前位置实例化该建筑，并将其作为子对象
                        Instantiate(BK[numB], bk.position, bk.rotation, bk);
                        // 增加建筑数量计数器
                        nB++;
                    }
                    else
                    {
                        // 在街区的四个角落创建建筑
                        for (int i = 1; i <= 4; i++)
                        {
                            // 创建一个新的 GameObject 作为标记
                            GameObject nc = new GameObject("E");
                            // 将标记 GameObject 设置为当前街区的子对象
                            nc.transform.SetParent(bk);
                            // 根据索引设置标记的位置和旋转
                            if (i == 1)
                            {
                                nc.transform.localPosition = new Vector3(-36, 0, -36);
                                nc.transform.localRotation = Quaternion.Euler(0, 180, 0);
                            }
                            if (i == 2)
                            {
                                nc.transform.localPosition = new Vector3(-36, 0, 36);
                                nc.transform.localRotation = Quaternion.Euler(0, 270, 0);
                            }
                            if (i == 3)
                            {
                                nc.transform.localPosition = new Vector3(36, 0, 36);
                                nc.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            }
                            if (i == 4)
                            {
                                nc.transform.localPosition = new Vector3(36, 0, -36);
                                nc.transform.localRotation = Quaternion.Euler(0, 90, 0);
                            }
                            // 在标记位置创建角落建筑
                            CreateBuildingsInCorners(nc);
                        }
                    }
                }
            }
        }

        // 在超级街区中创建建筑
        public void CreateBuildingsInSuperBlocks()
        {
            // 用于存储随机选择的建筑索引
            int numB = 0;

            // 查找场景中所有名称为 "SuperBlocks" 的 GameObject
            tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("SuperBlocks")).ToArray();

            // 遍历所有找到的 "SuperBlocks" GameObject
            foreach (GameObject bks in tempArray)
            {
                // 遍历 "SuperBlocks" GameObject 下的所有子对象
                foreach (Transform bk in bks.transform)
                {
                    // 用于记录尝试次数
                    int lp = 0;
                    do
                    {
                        lp++;
                        // 随机选择一个超级大型建筑的索引
                        numB = Random.Range(0, SB.Length);
                        // 如果该建筑的使用次数为 0，跳出循环
                        if (_SB[numB] == 0) break;
                        // 根据尝试次数和使用次数的条件跳出循环
                        if (lp > 125 && _SB[numB] <= 1) break;
                        if (lp > 150 && _SB[numB] <= 2) break;
                        if (lp > 200 && _SB[numB] <= 3) break;
                        if (lp > 250) break;
                    } while (lp < 300);

                    // 增加该建筑的使用次数
                    _SB[numB] += 1;

                    // 在当前位置实例化该建筑，并将其作为子对象
                    Instantiate(SB[numB], bk.position, bk.rotation, bk);
                    // 增加建筑数量计数器
                    nB++;
                }
            }
        }

        // 在一条线上创建建筑物
        // line: 表示线的 GameObject
        // angulo: 建筑物的旋转角度
        // slope: 表示是否在斜坡上，默认为 false
        private void CreateBuildingsInLine(GameObject line, float angulo, bool slope = false)
        {
            // 初始化建筑物索引为 -1
            int index = -1;
            // 用于存储创建的建筑物的数组，最大容量为 50
            GameObject[] pBuilding;
            pBuilding = new GameObject[50];

            // 线的长度限制
            float limit;
            // 获取线的名称
            string _name = line.name;

            // 如果在斜坡上，去掉名称的第一个字符
            _name = (slope) ? line.name.Substring(1) : line.name;

            // 如果名称包含小数点，解析出具体的长度值
            if (_name.Contains("."))
                limit = float.Parse(_name.Split('.')[0]) + float.Parse(_name.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, _name.Split('.')[1].Length));
            else
                // 否则直接将名称转换为长度值
                limit = float.Parse(_name);

            // 初始化起始位置
            float init = 0;
            // 建筑物的宽度
            float pWidth = 0;

            // 外层循环计数器，最多循环 100 次
            int tt = 0;
            // 内层循环计数器
            int t;
            // 尝试选择建筑物的次数计数器
            int lp;

            // 计算线的位置到城市中心的距离
            float distancia = Vector3.Distance(center, line.transform.position);

            // 计算市中心的有效距离，根据市中心大小进行调整
            float _distCenter = distCenter * (Mathf.Clamp(downTownSize, 50, 200) / 100);

            // 外层循环，最多尝试 100 次
            while (tt < 100)
            {
                tt++;
                t = 0;
                lp = 0;

                // 内层循环，最多尝试 200 次，并且起始位置不能超过线的长度减去 4
                while (t < 200 && init <= limit - 4)
                {
                    t++;

                    // 如果在斜坡上
                    if (slope)
                    {
                        // 如果在线的位置在市中心范围内且存在市中心区域
                        if (distancia < _distCenter && withDowntownArea)
                        {
                            // 尝试选择一个合适的市中心斜坡建筑物
                            do
                            {
                                lp++;
                                // 随机选择一个市中心斜坡建筑物的索引
                                numB = Random.Range(0, BCS.Length);
                                // 如果该建筑物未被使用过，跳出循环
                                if (_BCS[numB] == 0) break;
                                // 根据尝试次数和使用次数的条件跳出循环
                                if (lp > 125 && _BCS[numB] <= 1) break;
                                if (lp > 150 && _BCS[numB] <= 2) break;
                                if (lp > 200 && _BCS[numB] <= 3) break;
                                if (lp > 250) break;
                            } while (lp < 300);

                            // 获取选择的建筑物的宽度
                            pWidth = GetWith(BCS[numB]);

                            // 如果宽度大于 0 且加上该建筑物后不会超过线的长度加 4
                            if (pWidth > 0)
                                if ((init + pWidth) <= (limit + 4))
                                {
                                    // 选择该建筑物
                                    pB = BCS[numB];
                                    // 增加该建筑物的使用次数
                                    _BCS[numB] += 1;
                                    break;
                                }
                        }
                        else
                        {
                            // 尝试选择一个合适的郊区斜坡建筑物
                            do
                            {
                                lp++;
                                // 随机选择一个郊区斜坡建筑物的索引
                                numB = Random.Range(0, BBS.Length);
                                // 如果该建筑物未被使用过，跳出循环
                                if (_BBS[numB] == 0) break;
                                // 根据尝试次数和使用次数的条件跳出循环
                                if (lp > 125 && _BBS[numB] <= 1) break;
                                if (lp > 150 && _BBS[numB] <= 2) break;
                                if (lp > 200 && _BBS[numB] <= 3) break;
                                if (lp > 250) break;
                            } while (lp < 300);

                            // 获取选择的建筑物的宽度
                            pWidth = GetWith(BBS[numB]);

                            // 如果宽度大于 0 且加上该建筑物后不会超过线的长度加 4
                            if (pWidth > 0)
                                if ((init + pWidth) <= (limit + 4))
                                {
                                    // 选择该建筑物
                                    pB = BBS[numB];
                                    // 增加该建筑物的使用次数
                                    _BBS[numB] += 1;
                                    break;
                                }
                        }
                    }
                    // 如果不在斜坡上，且在线的位置在市中心范围内且存在市中心区域
                    else if (distancia < _distCenter && withDowntownArea)
                    {
                        // 尝试选择一个合适的市中心建筑物
                        do
                        {
                            lp++;
                            // 随机选择一个市中心建筑物的索引
                            numB = Random.Range(0, BC.Length);
                            // 如果该建筑物未被使用过，跳出循环
                            if (_BC[numB] == 0) break;
                            // 根据尝试次数和使用次数的条件跳出循环
                            if (lp > 125 && _BC[numB] <= 1) break;
                            if (lp > 150 && _BC[numB] <= 2) break;
                            if (lp > 200 && _BC[numB] <= 3) break;
                            if (lp > 250) break;
                        } while (lp < 300);

                        // 获取选择的建筑物的宽度
                        pWidth = GetWith(BC[numB]);

                        // 如果宽度大于 0 且加上该建筑物后不会超过线的长度加 4
                        if (pWidth > 0)
                            if ((init + pWidth) <= (limit + 4))
                            {
                                // 选择该建筑物
                                pB = BC[numB];
                                // 增加该建筑物的使用次数
                                _BC[numB] += 1;
                                break;
                            }
                    }
                    // 如果是住宅区
                    else if (_residential)
                    {
                        // 尝试选择一个合适的住宅建筑物
                        do
                        {
                            lp++;
                            // 随机选择一个住宅建筑物的索引
                            numB = Random.Range(0, BR.Length);
                            // 如果该建筑物未被使用过，跳出循环
                            if (_BR[numB] == 0) break;
                            // 根据尝试次数和使用次数的条件跳出循环
                            if (lp > 100 && _BR[numB] <= 1) break;
                            if (lp > 150 && _BR[numB] <= 2) break;
                            if (lp > 200 && _BR[numB] <= 3) break;
                            if (lp > 250) break;
                        } while (lp < 300);

                        // 获取选择的建筑物的宽度
                        pWidth = GetWith(BR[numB]);

                        // 如果宽度小于等于 0，输出警告信息并增加该建筑物的使用次数
                        if (pWidth <= 0) { Debug.LogWarning("Error: BR: " + numB); _BR[numB] += 1; }
                        else
                            // 如果宽度大于 0 且加上该建筑物后不会超过线的长度加 4
                            if ((init + pWidth) <= (limit + 4))
                        {
                            // 选择该建筑物
                            pB = BR[numB];
                            // 增加该建筑物的使用次数
                            _BR[numB] += 1;
                            // 增加住宅数量
                            residential += 1;
                            break;
                        }
                    }
                    else
                    {
                        // 尝试选择一个合适的郊区建筑物
                        do
                        {
                            lp++;
                            // 随机选择一个郊区建筑物的索引
                            numB = Random.Range(0, BB.Length);
                            // 如果该建筑物未被使用过，跳出循环
                            if (_BB[numB] == 0) break;
                            // 根据尝试次数和使用次数的条件跳出循环
                            if (lp > 100 && _BB[numB] <= 1) break;
                            if (lp > 150 && _BB[numB] <= 2) break;
                            if (lp > 200 && _BB[numB] <= 3) break;
                            if (lp > 250) break;
                        } while (lp < 300);

                        // 获取选择的建筑物的宽度
                        pWidth = GetWith(BB[numB]);

                        // 如果宽度小于等于 0，输出警告信息并增加该建筑物的使用次数
                        if (pWidth <= 0) { Debug.LogWarning("Error: BB: " + numB); _BB[numB] += 1; }
                        // 如果宽度大于 0 且加上该建筑物后不会超过线的长度加 4
                        if ((init + pWidth) <= (limit + 4))
                        {
                            // 选择该建筑物
                            pB = BB[numB];
                            // 增加该建筑物的使用次数
                            _BB[numB] += 1;
                            break;
                        }
                    }
                }

                // 如果内层循环尝试次数达到 200 次或者起始位置超过线的长度减去 4
                if (t >= 200 || init > limit - 4)
                {
                    // 没有找到合适的建筑物来填充剩余空间，调整已创建建筑物的宽度
                    AdjustsWidth(pBuilding, index + 1, limit - init, 0, slope);
                    break;
                }
                else
                {
                    // 找到合适的建筑物，增加索引
                    index++;
                    // 增加建筑物总数
                    nB++;

                    // 实例化选择的建筑物，并设置其位置和旋转
                    pBuilding[index] = (GameObject)Instantiate(pB, new Vector3(0, 0, init + (pWidth * 0.5f)), Quaternion.Euler(0, angulo, 0));
                    // 将建筑物设置为线的子对象
                    pBuilding[index].transform.SetParent(line.transform);

                    // 设置建筑物的本地位置
                    pBuilding[index].transform.localPosition = new Vector3(0, 0, init + (pWidth * 0.5f));
                    // 设置建筑物的本地旋转
                    pBuilding[index].transform.localRotation = Quaternion.Euler(0, angulo, 0);

                    // 更新起始位置
                    init += pWidth;

                    // 如果起始位置超过线的长度减去 6
                    if (init > limit - 6)
                    {
                        // 调整已创建建筑物的宽度
                        AdjustsWidth(pBuilding, index + 1, limit - init, 0, slope);
                        break;
                    }
                }
            }
        }

        // 根据给定的位置和宽度，通过射线检测获取合适的 Y 坐标
        // pos: 检测的起始位置的 Transform
        // width: 检测的宽度
        // 返回值: 合适的 Y 坐标
        private float GetY(Transform pos, float width)
        {
            // 用于存储射线检测的结果
            RaycastHit hit;

            // 射线的起始位置，向上和向前偏移一定距离
            Vector3 pp = pos.transform.position + pos.transform.forward * 2 + pos.transform.up * 20;

            // 右侧射线检测到的距离
            float l = 20;
            // 左侧射线检测到的距离
            float r = 20;

            // 向右偏移宽度后向下发射射线进行检测
            if (Physics.Raycast(pp + pos.transform.right * width, Vector3.down, out hit, 40))
                r = hit.distance;

            // 向左偏移宽度后向下发射射线进行检测
            if (Physics.Raycast(pp - (pos.transform.right * width), Vector3.down, out hit, 40))
                l = hit.distance;

            // 返回合适的 Y 坐标，取左右两侧检测距离的较小值
            return (pos.transform.localPosition.y + 20) - ((r < l) ? r : l);
        }

        // 在双行区域创建建筑物
        private void CreateBuildingsInDoubleLine(GameObject line)
        {
            // 初始化建筑物索引为 -1
            int index = -1;
            // 用于存储创建的建筑物的数组，最多可存储 20 个
            GameObject[] pBuilding;
            pBuilding = new GameObject[20];

            // 定义双行区域的长度限制
            float limit;
            // 获取双行区域对象的名称
            string _name = line.name;

            // 如果名称包含小数点，将名称拆分为整数部分和小数部分并转换为浮点数
            if (_name.Contains("."))
                limit = float.Parse(_name.Split('.')[0]) + float.Parse(_name.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, _name.Split('.')[1].Length));
            else
                // 否则直接将名称转换为浮点数
                limit = float.Parse(_name);

            // 初始化建筑物放置的起始位置
            float init = 0;
            // 建筑物的宽度
            float pWidth = 0;

            // 循环计数器，最多尝试 100 次
            int tt = 0;
            // 内层循环计数器
            int t;
            // 选择建筑物的尝试次数计数器
            int lp;

            // 最多尝试 100 次来放置建筑物
            while (tt < 100)
            {
                tt++;
                t = 0;
                lp = 0;

                // 内层循环，最多尝试 200 次，且起始位置不能超过限制减去 4
                while (t < 200 && init <= limit - 4)
                {
                    t++;

                    // 循环选择合适的建筑物，最多尝试 300 次
                    do
                    {
                        lp++;
                        // 随机选择一个 MB 数组中的建筑物索引
                        numB = Random.Range(0, MB.Length);
                        // 如果该建筑物的使用次数为 0，跳出循环
                        if (_MB[numB] == 0) break;
                        // 如果尝试次数超过 100 且该建筑物使用次数小于等于 1，跳出循环
                        if (lp > 100 && _MB[numB] <= 1) break;
                        // 如果尝试次数超过 150 且该建筑物使用次数小于等于 2，跳出循环
                        if (lp > 150 && _MB[numB] <= 2) break;
                        // 如果尝试次数超过 200，跳出循环
                        if (lp > 200) break;
                    } while (lp < 300);

                    // 获取所选建筑物的宽度
                    pWidth = GetWith(MB[numB]);

                    // 如果宽度小于等于 0，输出警告信息并增加该建筑物的使用次数
                    if (pWidth <= 0) { Debug.LogWarning("Error: MB: " + numB); _MB[numB] += 1; }
                    else if ((init + pWidth) <= (limit + 4))
                    {
                        // 如果放置该建筑物后不会超出限制，增加该建筑物的使用次数并跳出内层循环
                        _MB[numB] += 1;
                        break;
                    }
                }

                // 如果内层循环尝试次数达到 200 或者起始位置超过限制减去 4，调整建筑物宽度并跳出外层循环
                if (t >= 200 || init > limit - 4)
                {
                    AdjustsWidth(pBuilding, index + 1, (limit - init), 0);
                    break;
                }
                else
                {
                    // 否则，增加建筑物索引
                    index++;

                    // 实例化所选建筑物并将其作为子对象添加到双行区域对象中
                    pBuilding[index] = (GameObject)Instantiate(MB[numB], new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0), line.transform);
                    // 增加建筑物总数
                    nB++;

                    // 设置建筑物的名称
                    pBuilding[index].name = "building";
                    // 设置建筑物的父对象
                    pBuilding[index].transform.SetParent(line.transform);
                    // 设置建筑物的本地位置
                    pBuilding[index].transform.localPosition = new Vector3(0, 0, (init + (pWidth * 0.5f)));
                    // 设置建筑物的本地旋转
                    pBuilding[index].transform.localRotation = Quaternion.Euler(0, 90, 0);

                    // 更新起始位置
                    init += pWidth;

                    // 如果起始位置超过限制减去 6，调整建筑物宽度
                    if (init > limit - 6)
                    {
                        AdjustsWidth(pBuilding, index + 1, (limit - init), 0);
                    }
                }
            }
        }

        // 在双行区域创建建筑物的主方法
        private void CreateBuildingsInDouble()
        {
            // 定义双行区域的长度限制
            float limit;

            // 查找场景中所有名称为 "Double" 的对象
            tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == ("Double")).ToArray();

            // 用于存储双行交叉区域的对象
            GameObject DB;
            // 用于存储标记对象
            GameObject mc2;
            // 用于存储主标记对象
            GameObject mc;

            // 遍历所有双行交叉区域对象
            foreach (GameObject dbCross in tempArray)
            {
                // 遍历双行交叉区域对象的所有子对象
                foreach (Transform line in dbCross.transform)
                {
                    // 如果子对象名称包含小数点，将名称拆分为整数部分和小数部分并转换为浮点数
                    if (line.name.Contains("."))
                        limit = float.Parse(line.name.Split('.')[0]) + float.Parse(line.name.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, line.name.Split('.')[1].Length));
                    else
                        // 否则直接将名称转换为浮点数
                        limit = float.Parse(line.name);

                    // 随机判断是否放置块状建筑物
                    if (Random.Range(0, 10) < 5)
                    {
                        // 块状建筑物

                        // 第一个块状建筑物的高度
                        float wl;
                        // 第二个块状建筑物的高度
                        float wl2;

                        // 循环选择合适的 DC 数组中的建筑物，直到其高度小于等于限制的一半
                        do
                        {
                            numB = Random.Range(0, DC.Length);
                            wl = GetHeight(DC[numB]);
                        } while (wl > limit / 2);

                        // 实例化第一个块状建筑物并将其作为子对象添加到当前行对象中
                        GameObject e = (GameObject)Instantiate(DC[numB], line.transform.position, line.transform.rotation, line.transform);
                        // 增加建筑物总数
                        nB++;

                        // 循环选择合适的 DC 数组中的建筑物，直到其高度小于等于限制减去第一个建筑物高度和 26
                        do
                        {
                            numB = Random.Range(0, DC.Length);
                            wl2 = GetHeight(DC[numB]);
                        } while (wl2 > limit - (wl + 26));

                        // 实例化第二个块状建筑物并将其作为子对象添加到当前行对象中
                        e = (GameObject)Instantiate(DC[numB], line.transform.position, line.rotation, line.transform);
                        // 设置第二个建筑物的父对象
                        e.transform.SetParent(line.transform);
                        // 设置第二个建筑物的本地位置
                        e.transform.localPosition = new Vector3(0, 0, -limit);
                        // 设置第二个建筑物的本地旋转
                        e.transform.localRotation = Quaternion.Euler(0, 180, 0);

                        // 创建一个新的对象用于存储剩余空间的长度
                        DB = new GameObject("" + ((limit - wl - wl2)));
                        // 设置新对象的父对象
                        DB.transform.SetParent(line.transform);
                        // 设置新对象的本地位置
                        DB.transform.localPosition = new Vector3(0, 0, -(limit - wl2));
                        // 设置新对象的本地旋转
                        DB.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        // 设置新对象的名称
                        DB.name = "" + ((limit - wl - wl2));

                        // 在新对象中创建双行建筑物
                        CreateBuildingsInDoubleLine(DB);
                    }
                    else
                    {
                        // 线条和角落

                        // 创建一个主标记对象
                        mc = new GameObject("Marcador");
                        // 设置主标记对象的父对象
                        mc.transform.SetParent(line);
                        // 设置主标记对象的本地位置
                        mc.transform.localPosition = new Vector3(0, 0, 0);
                        // 设置主标记对象的本地旋转
                        mc.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        // 循环创建 4 个角落标记对象
                        for (int i = 1; i <= 4; i++)
                        {
                            // 创建一个角落标记对象
                            mc2 = new GameObject("E");
                            // 设置角落标记对象的父对象
                            mc2.transform.SetParent(mc.transform);

                            // 根据循环索引设置角落标记对象的本地位置和旋转
                            if (i == 1)
                            {
                                mc2.transform.localPosition = new Vector3(36, 0, -limit);
                                mc2.transform.localRotation = Quaternion.Euler(0, 90, 0);
                            }
                            if (i == 2)
                            {
                                mc2.transform.localPosition = new Vector3(36, 0, 0);
                                mc2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            }
                            if (i == 3)
                            {
                                mc2.transform.localPosition = new Vector3(-36, 0, 0);
                                mc2.transform.localRotation = Quaternion.Euler(0, 270, 0);
                            }
                            if (i == 4)
                            {
                                mc2.transform.localPosition = new Vector3(-36, 0, -limit);
                                mc2.transform.localRotation = Quaternion.Euler(0, 180, 0);
                            }

                            // 在角落标记对象中创建角落建筑物
                            CreateBuildingsInCorners(mc2);
                        }

                        // 创建一个新对象用于存储剩余空间的长度
                        mc2 = new GameObject("" + (limit - 72));
                        // 设置新对象的父对象
                        mc2.transform.SetParent(mc.transform);
                        // 设置新对象的本地位置
                        mc2.transform.localPosition = new Vector3(-36, 0.001f, -36);
                        // 设置新对象的本地旋转
                        mc2.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        // 在新对象中创建单行建筑物
                        CreateBuildingsInLine(mc2, 90f);

                        // 创建另一个新对象用于存储剩余空间的长度
                        mc2 = new GameObject("" + (limit - 72));
                        // 设置新对象的父对象
                        mc2.transform.SetParent(mc.transform);
                        // 设置新对象的本地位置
                        mc2.transform.localPosition = new Vector3(36, 0.001f, -(limit - 36));
                        // 设置新对象的本地旋转
                        mc2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        // 在新对象中创建单行建筑物
                        CreateBuildingsInLine(mc2, 90f);
                    }
                }
            }
        }

        // 调整建筑物的宽度以适应剩余空间
        private void AdjustsWidth(GameObject[] tBuildings, int quantity, float remainingMeters, float init, bool slope = false)
        {
            // 如果剩余空间为 0，直接返回
            if (remainingMeters == 0)
                return;

            // 计算每个建筑物需要调整的宽度
            float ajuste = remainingMeters / quantity;

            // 初始化起始位置
            float zInit = init;
            // 建筑物的宽度
            float pWidth;
            // 建筑物的缩放比例
            float pScale;
            // 建筑物的原始宽度
            float gw;

            // 遍历所有需要调整的建筑物
            for (int i = 0; i < quantity; i++)
            {
                // 获取建筑物的原始宽度
                gw = GetWith(tBuildings[i]);

                // 如果原始宽度大于 0
                if (gw > 0)
                {
                    // 计算建筑物的缩放比例
                    pScale = 1 + (ajuste / gw);
                    // 计算调整后的建筑物宽度
                    pWidth = gw + ajuste;

                    // 设置建筑物的本地位置
                    tBuildings[i].transform.localPosition = new Vector3(tBuildings[i].transform.localPosition.x, tBuildings[i].transform.localPosition.y, zInit + (pWidth * 0.5f));
                    // 设置建筑物的本地缩放
                    tBuildings[i].transform.localScale = new Vector3(pScale, 1, 1);
                    // 更新起始位置
                    zInit += pWidth;

                    // 如果在斜坡上
                    if (slope)
                    {
                        // 计算建筑物的高度调整值
                        float p;
                        p = GetY(tBuildings[i].transform, (gw * pScale) * 0.5f);
                        // 调整建筑物的位置
                        tBuildings[i].transform.position += new Vector3(0, p, 0);
                    }
                }
            }
        }

        // 获取建筑物的宽度
        private float GetWith(GameObject building)
        {
            // 如果建筑物对象为空，返回 0
            if (!building)
                return 0;

            // 如果建筑物对象有 MeshFilter 组件
            if (building.transform.GetComponent<MeshFilter>() != null)
            {
                // 如果 MeshFilter 组件的共享网格为空，输出错误信息
                if (building.transform.GetComponent<MeshFilter>().sharedMesh == null)
                {
                    Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
                    //return 0;
                }

                // 返回建筑物的宽度
                return building.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            }
            else
            {
                // 如果没有 MeshFilter 组件，输出错误信息并返回 0
                Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
                return 0;
            }
        }

        // 获取建筑物的高度
        private float GetHeight(GameObject building)
        {
            // 如果建筑物对象有 MeshFilter 组件
            if (building.GetComponent<MeshFilter>() != null)
                // 返回建筑物的高度
                return building.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;
            else
            {
                // 如果没有 MeshFilter 组件，输出错误信息并返回 0
                Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
                return 0;
            }
        }

        // 销毁场景中的建筑物
        public void DestroyBuildings()
        {
            // 销毁所有名称为 "Marcador" 的对象及其子对象
            DestryObjetcs("Marcador");
            // 销毁所有名称为 "Blocks" 的对象及其子对象
            DestryObjetcs("Blocks");
            // 销毁所有名称为 "SuperBlocks" 的对象及其子对象
            DestryObjetcs("SuperBlocks");
            // 销毁所有名称为 "Double" 的对象及其子对象
            DestryObjetcs("Double");
        }

        // 销毁指定名称的对象及其子对象
        private void DestryObjetcs(string tag)
        {
            // 查找场景中所有名称为指定标签的对象
            tempArray = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name == (tag)).ToArray();

            // 遍历所有找到的对象
            foreach (GameObject objt in tempArray)
                // 遍历对象的所有子对象
                foreach (Transform child in objt.transform)
                    // 从后往前遍历子对象的所有子对象并销毁
                    for (int k = child.childCount - 1; k >= 0; k--)
                        DestroyImmediate(child.GetChild(k).gameObject);
        }


    }
}