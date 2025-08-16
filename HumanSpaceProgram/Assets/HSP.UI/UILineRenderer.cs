using UnityEngine;
using UnityEngine.UI;

namespace HSP.UI
{
    /// <summary>
    /// Draws a polyline using the Unity the UI system.
    /// </summary>
    [RequireComponent( typeof( CanvasRenderer ) )]
	public class UILineRenderer : MaskableGraphic
	{
        [SerializeField]
		private Vector2[] _points;
		/// <summary>
		/// The points that define the line.
		/// </summary>
		public Vector2[] Points
		{
			get => _points;
			set
			{
				_points = value;
				this.SetVerticesDirty();
			} 
		}

        [SerializeField]
        private float _thickness = 10f;
		/// <summary>
		/// The thickness, in [px].
		/// </summary>
		public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                this.SetVerticesDirty();
            }
        }

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			vh.Clear();

			if( _points == null || _points.Length < 2 )
				return;

			if( _thickness == 0 )
				return;

			for( int i = 0; i < _points.Length - 1; i++ )
			{
				CreateLineSegment( _points[i], _points[i + 1], vh );

				int startVertIndex = i * 5;

				vh.AddTriangle( startVertIndex, startVertIndex + 1, startVertIndex + 3 );
				vh.AddTriangle( startVertIndex + 3, startVertIndex + 2, startVertIndex );

				// This is used to fill the gap between two lines, when the lines are rotated.
				if( i != 0 )
				{
					vh.AddTriangle( startVertIndex, startVertIndex - 1, startVertIndex - 3 );
					vh.AddTriangle( startVertIndex + 1, startVertIndex - 1, startVertIndex - 2 );
				}
			}
		}

		private void CreateLineSegment( Vector3 p1, Vector3 p2, VertexHelper vh )
		{
			UIVertex vertex = UIVertex.simpleVert;
			vertex.color = color;

			Quaternion point1Rotation = Quaternion.Euler( 0, 0, GetAngle( p1, p2 ) + 90 );
			vertex.position = point1Rotation * new Vector3( -_thickness / 2, 0 );
			vertex.position += p1;
			vh.AddVert( vertex );
			vertex.position = point1Rotation * new Vector3( _thickness / 2, 0 );
			vertex.position += p1;
			vh.AddVert( vertex );

			Quaternion point2Rotation = Quaternion.Euler( 0, 0, GetAngle( p2, p1 ) - 90 );
			vertex.position = point2Rotation * new Vector3( -_thickness / 2, 0 );
			vertex.position += p2;
			vh.AddVert( vertex );
			vertex.position = point2Rotation * new Vector3( _thickness / 2, 0 );
			vertex.position += p2;
			vh.AddVert( vertex );

			// This is used to fill the gap between two lines, when the lines are rotated.
			vertex.position = p2;
			vh.AddVert( vertex );
		}

		private static float GetAngle( Vector2 vertex, Vector2 target )
		{
			return Mathf.Atan2( target.y - vertex.y, target.x - vertex.x ) * (180 / Mathf.PI);
		}
	}
}