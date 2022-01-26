using System;
using UnityEngine;
using System.Collections.Generic;

namespace XYModLib
{
    /// <summary>
    /// 通用搜索GUI，带一个搜索条，和一个ScrollView用来显示结果
    /// </summary>
    public class SearchGUI<T>
    {
        private List<T> searchResultList = new List<T>();
        private List<T> searchSource = new List<T>();
        private string searchStr = "";
        private int nowPage;
        private int maxPage;
        private int tmpPage;
        private int tmpShow;
        private Vector2 sv;

        private int perPageNum;
        private int searchInputWidth;
        private Func<T, string, bool> searchFunc;
        private Func<List<T>> getSearchObjFunc;
        private Action<T> showObjGUIAction;

        /// <summary>
        /// 开启搜索结果SV的皮肤
        /// </summary>
        public bool EnableSVSkin;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="getSearchObjFunc">获取搜索源的委托</param>
        /// <param name="searchFunc">搜索方法委托</param>
        /// <param name="showObjGUIAction">显示搜索结果每个项目的GUI委托</param>
        /// <param name="perPageNum">每页显示数量</param>
        /// <param name="searchInputWidth">搜索条宽度</param>
        public SearchGUI(Func<List<T>> getSearchObjFunc, Func<T, string, bool> searchFunc, Action<T> showObjGUIAction, int perPageNum = 10, int searchInputWidth = 100)
        {
            this.perPageNum = perPageNum;
            this.searchFunc = searchFunc;
            this.showObjGUIAction = showObjGUIAction;
            this.getSearchObjFunc = getSearchObjFunc;
            this.searchInputWidth = searchInputWidth;
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("搜索");
            searchStr = GUILayout.TextField(searchStr, GUILayout.Width(searchInputWidth));
            GUILayout.FlexibleSpace();
            // 准备所有可搜索对象
            searchSource = getSearchObjFunc();
            // 清空结果池
            searchResultList.Clear();
            // 搜索
            foreach (T obj in searchSource)
            {
                if (searchFunc(obj, searchStr))
                {
                    searchResultList.Add(obj);
                }
            }
            // 计算页数
            maxPage = searchResultList.Count / perPageNum;
            if (searchResultList.Count % perPageNum != 0) maxPage++;
            // 显示页数选择
            GUILayout.Label($"  第{nowPage + 1}页 共{maxPage}页", GUILayout.Width(100));
            if (GUILayout.Button("上一页", GUILayout.Width(50))) nowPage--;
            if (GUILayout.Button("下一页", GUILayout.Width(50))) nowPage++;
            if (nowPage < 0) nowPage = maxPage - 1;
            if (nowPage >= maxPage) nowPage = 0;
            tmpPage = 0;
            tmpShow = 0;
            GUILayout.EndHorizontal();

            // 渲染搜索结果
            if (EnableSVSkin)
            {
                sv = GUILayout.BeginScrollView(sv, GUI.skin.box);
            }
            else
            {
                sv = GUILayout.BeginScrollView(sv);
            }
            foreach (T obj in searchResultList)
            {
                if (tmpPage < nowPage * perPageNum)
                {
                    tmpPage++;
                    continue;
                }
                tmpShow++;
                showObjGUIAction(obj);
                if (tmpShow >= perPageNum)
                {
                    break;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
