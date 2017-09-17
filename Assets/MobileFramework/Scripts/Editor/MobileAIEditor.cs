#region Using Statements
using UnityEngine;
using UnityEditor;
using MobileAIFramework.Navigation;
#endregion

namespace MobileAIFramework.Editors
{
	[CustomEditor(typeof(MobileAI))]
	public class MobileAIEditor : Editor
	{
		//A reference to the MobileAI class
		private MobileAI m_editor = null;
		private bool m_ready = false;
		
		//Define whenever a particular section group of the editor shuld be drawn
		private bool m_showDebug	= true;
		private bool m_showVolume 	= true;
		private bool m_showGridInfo = true;
		
		//Serialized properties
		//Debug Fold
		private SerializedProperty m_debugGrid;
		private SerializedProperty m_drawMapArea;
		private SerializedProperty m_drawTrail;
		
		//Volume Fold
		private SerializedProperty m_volumeSize;
		private SerializedProperty m_gridSize;
		
		private SerializedProperty m_camera;
		
		private SerializedProperty m_nonBlocking;
		private SerializedProperty m_trailDebug;
		
		//Grid Info Fold
		//private SerializedProperty m_dynamicObstacles;
		
		//When the object is enabled, this method is called.
		//Hides all the not customizable component of the MobileAI Framework component
		void OnEnable()
		{
			if (m_editor != null)
				return;
		
			//Get the list of components and hide the unnecessary ones
			if (target is MobileAI)
			{
				m_editor = target as MobileAI;
			
				m_editor.GetComponent<NavArea>().hideFlags 		= HideFlags.HideInInspector;
				m_editor.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
				m_editor.GetComponent<MeshFilter>().hideFlags 	= HideFlags.HideInInspector;
				
				m_debugGrid 	= serializedObject.FindProperty("m_drawGrid");
				m_drawMapArea 	= serializedObject.FindProperty("m_drawMapArea");
				m_volumeSize 	= serializedObject.FindProperty("m_volumeSize");
				m_gridSize		= serializedObject.FindProperty("m_gridSize");
				
				m_nonBlocking 	= serializedObject.FindProperty("m_nonBlockingObjects");
				
				m_camera		= serializedObject.FindProperty("m_targetCamera");
				
				m_drawTrail 	= serializedObject.FindProperty("m_drawTrail");
				m_trailDebug	= serializedObject.FindProperty("m_trailDebug");
				
				m_ready = true;
			}
		}
	
		//Change the apperence of the MobileAI component on the inspector of unity
		//providing an advanced and more optimized and more customizable interface of the framework.
		public override void OnInspectorGUI()
		{
			if (!m_ready) return;
		
			//If you would like to ADD to the inspector,
			// rather than just override it, use this next line:
			//DrawDefaultInspector();
			
			//Check if the target is the MobileAI itself
			m_editor = target as MobileAI;
			
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.HelpBox("MobileAI Framework.", MessageType.None);
			
			//Volume Foldout
			if (m_showVolume = EditorGUILayout.Foldout(m_showVolume, "Volume and Grid"))
			{
				EditorGUILayout.HelpBox("This is the volume where the plugin traces the navigation grid.", MessageType.Info);
				
				EditorGUI.indentLevel++;
				
				//Change the volume
				m_volumeSize.vector3Value 	= EditorGUILayout.Vector3Field("Volume Area:", m_volumeSize.vector3Value);
				
				EditorGUILayout.Separator();
				
				EditorGUI.indentLevel--;
			}
			
			//Grid Information Foldout
			if (m_showGridInfo = EditorGUILayout.Foldout(m_showGridInfo, "Grid Information"))
			{
				EditorGUILayout.HelpBox("Number of subdivisions within the volume.", MessageType.Info);
			
				EditorGUI.indentLevel++;
				
				//Change the volume
				m_gridSize.vector2Value = EditorGUILayout.Vector2Field("Grid Subdivision:", m_gridSize.vector2Value);
				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(m_nonBlocking, true);
				
				EditorGUILayout.HelpBox("Defines the camera where the screen raycast is calculated.", MessageType.Info);
				EditorGUILayout.PropertyField(m_camera, true);
				
				EditorGUI.indentLevel--;
			}
			
			if (m_showDebug = EditorGUILayout.Foldout(m_showDebug, "Debug"))
			{
				EditorGUI.indentLevel++;
				
				m_debugGrid.boolValue 	= EditorGUILayout.Toggle("Draw Grid", m_debugGrid.boolValue);
				m_drawMapArea.boolValue = EditorGUILayout.Toggle("Draw Volume", m_drawMapArea.boolValue);
				
				bool oldDrawTail = m_editor.DrawTrail;
				m_drawTrail.boolValue = EditorGUILayout.Toggle("Draw Trails", m_drawTrail.boolValue);
				
				if (oldDrawTail != m_drawTrail.boolValue)
					m_editor.DrawTrail = m_drawTrail.boolValue;
				
				if (m_drawTrail.boolValue)
				{
					EditorGUILayout.HelpBox("The TrailRenderer used to debug.", MessageType.Info);
					EditorGUILayout.PropertyField(m_trailDebug, true);
				}
				
				EditorGUI.indentLevel--;
			}
			
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			if(GUILayout.Button("Default Values"))
			{
				m_editor.transform.position = Vector3.zero;
				m_volumeSize.vector3Value 	= new Vector3(32, 32, 32);
				m_gridSize.vector2Value 	= new Vector2(32, 32);
			}
			
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
	}
}