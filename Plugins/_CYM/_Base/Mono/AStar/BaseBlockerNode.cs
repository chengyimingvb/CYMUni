//------------------------------------------------------------------------------
// BaseBlockerNode.cs
// Copyright 2019 2019/5/5 
// Created by CYM on 2019/5/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Pathfinding;
using UnityEngine;

namespace CYM
{
    public class BaseBlockerNode : MonoBehaviour
    {
        public GraphNode Node { get; private set; }
        protected BaseSceneRoot BaseSceneObject => BaseSceneRoot.Ins;
        AstarPath AstarPath => AstarPath.active;

        private void Start()
        {
            Node = AstarPath.GetNearest(transform.position, NNConstraint.None).node;
        }
    }
}