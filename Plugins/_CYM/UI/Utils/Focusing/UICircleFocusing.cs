using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
	/// <summary>
	/// 圆形遮罩镂空引导
	/// </summary>
	public class UICircleFocusing :MonoBehaviour, ICanvasRaycastFilter
	{
        #region prop
        /// <summary>
        /// 要高亮显示的目标
        /// </summary>
        public Image Target { get;private set; }
		/// <summary>
		/// 镂空区域圆心
		/// </summary>
		private Vector4 _center;
		/// <summary>
		/// 镂空区域半径
		/// </summary>
		private float _radius;
		/// <summary>
		/// 遮罩材质
		/// </summary>
		private Material _material;
		/// <summary>
		/// 当前高亮区域的半径
		/// </summary>
		private float _currentRadius;
		/// <summary>
		/// 高亮区域缩放的动画时间
		/// </summary>
		private float _shrinkTime = 0.2f;
		/// <summary>
		/// 收缩速度
		/// </summary>
		private float _shrinkVelocity = 0f;
		#endregion

		#region is
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (Target == null)
				return true;
			//return !RectTransformUtility.RectangleContainsScreenPoint(Target.rectTransform, sp, eventCamera);

			if (_currentRadius == 0f)
				return false;

			RectTransform rectTransform = Target.rectTransform;
			Vector2 localPositionPivotRelative;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPositionPivotRelative);
			bool ret = Vector2.Distance(localPositionPivotRelative, _center) > _currentRadius;
			return ret;
		}
        #endregion

        #region life
        void Awake()
		{
			Target = GetComponent<Image>();
			_material = GameObject.Instantiate(Target.material);
			Target.material = _material;
			SetCenter(Vector4.zero);
			SetRadius(0);
		}
		void Update()
		{
			//从当前半径到目标半径差值显示收缩动画
			float value = Mathf.SmoothDamp(_currentRadius, _radius, ref _shrinkVelocity, _shrinkTime);
			if (!Mathf.Approximately(value, _currentRadius))
			{
				_currentRadius = value;
				_material.SetFloat("_Slider", _currentRadius);
			}
		}
		#endregion

		#region pub
		public void SetCenter(Vector2 pos)
		{
			_center = pos;
			_material.SetVector("_Center", _center);
		}
		public void SetRadius(float val)
		{
			_currentRadius = val * 2;
			   _radius = val;
			//_material.SetFloat("_Slider", _currentRadius);
		}
        #endregion
    }

}