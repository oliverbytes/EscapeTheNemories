#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class Uni2DEditorUtilsPolygonTriangulation
{
	// Return a polygonized mesh from 2D outer/inner contours
	public static Mesh EarClipping( List<Contour> a_rDominantContoursList, float a_fExtrusionDepth, float a_fScale, Vector3 a_f3PivotPoint )
	{
		// The mesh to build
		Mesh oCombinedMesh = new Mesh( );

		int iContourCount = a_rDominantContoursList.Count; //a_rDominantContoursList.Count;

		// Step 2: Ear clip outer contour
		CombineInstance[ ] oCombineMeshInstance = new CombineInstance[ iContourCount ];
		for( int iContourIndex = 0; iContourIndex < iContourCount; ++iContourIndex )
		{
			Vector3[ ] oVerticesArray;
			List<int> oTrianglesList;

			EarClippingSubMesh( a_rDominantContoursList[ iContourIndex ], a_fScale, a_f3PivotPoint, out oVerticesArray, out oTrianglesList );

			oCombineMeshInstance[ iContourIndex ].mesh = BuildExtrudedMeshFromPolygonizedContours( oVerticesArray, oTrianglesList, a_fExtrusionDepth ); // EarClippingSubMesh( a_rDominantContoursList[ iContourIndex ] );
		}

		// Step 3: Combine every sub meshes (merge, no transform)
		oCombinedMesh.CombineMeshes( oCombineMeshInstance, true, false );

		// Return!
		return oCombinedMesh;
	}

	public static List<Mesh> EarClippingCompound( List<Contour> a_rDominantContoursList, float a_fExtrusionDepth, float a_fScale, Vector3 a_f3PivotPoint )
	{
		List<Mesh> oMeshesList = new List<Mesh>( );

		foreach( Contour rDominantContour in a_rDominantContoursList )
		{
			Vector3[ ] oVerticesArray;
			List<int> oTrianglesList;

			EarClippingSubMesh( rDominantContour, a_fScale, a_f3PivotPoint, out oVerticesArray, out oTrianglesList );
			List<Mesh> rExtrudedTriangleMeshesList = BuildExtrudedTrianglesFromPolygonizedContours( oVerticesArray, oTrianglesList, a_fExtrusionDepth );
			oMeshesList.AddRange( rExtrudedTriangleMeshesList );
		}

		return oMeshesList;
	}
	
	// Return a polygonized mesh from a 2D outer contour
	private static void EarClippingSubMesh( Contour a_rDominantOuterContour, float a_fScale, Vector3 a_f3PivotPoint, out Vector3[ ] a_rVerticesArray, out List<int> a_rTrianglesList )
	{
		// Sum of all contours count
		int iVerticesCount = a_rDominantOuterContour.Count;

		// Mesh vertices array
		a_rVerticesArray = new Vector3[ iVerticesCount ];

		// Vertex indexes lists array (used by ear clipping algorithm)
		CircularLinkedList<int> oVertexIndexesList = new CircularLinkedList<int>( );

		// Build contour vertex index circular list
		// Store every Vector3 into mesh vertices array
		// Store corresponding index into the circular list
		int iVertexIndex = 0;
		foreach( Vector2 f2OuterContourVertex in a_rDominantOuterContour.Vertices )
		{
			a_rVerticesArray[ iVertexIndex ] = f2OuterContourVertex;
			oVertexIndexesList.AddLast( iVertexIndex );

			++iVertexIndex;
		}

		// Build reflex/convex vertices lists
		LinkedList<int> rReflexVertexIndexesList;
		LinkedList<int> rConvexVertexIndexesList;
		BuildReflexConvexVertexIndexesLists( a_rVerticesArray, oVertexIndexesList, out rReflexVertexIndexesList, out rConvexVertexIndexesList );

		// Triangles for this contour
		a_rTrianglesList = new List<int>( );

		// Build ear tips list
		CircularLinkedList<int> rEarTipVertexIndexesList = BuildEarTipVerticesList( a_rVerticesArray, oVertexIndexesList, rReflexVertexIndexesList, rConvexVertexIndexesList );

		// Remove the ear tips one by one!
		while( rEarTipVertexIndexesList.Count > 0 && oVertexIndexesList.Count > 2 )
		{

			CircularLinkedListNode<int> rEarTipNode = rEarTipVertexIndexesList.First;

			// Ear tip index
			int iEarTipVertexIndex = rEarTipNode.Value;

			// Ear vertex indexes
			CircularLinkedListNode<int> rContourVertexNode                 = oVertexIndexesList.Find( iEarTipVertexIndex );
			CircularLinkedListNode<int> rPreviousAdjacentContourVertexNode = rContourVertexNode.Previous;
			CircularLinkedListNode<int> rNextAdjacentContourVertexNode     = rContourVertexNode.Next;

			int iPreviousAjdacentContourVertexIndex = rPreviousAdjacentContourVertexNode.Value;
			int iNextAdjacentContourVertexIndex     = rNextAdjacentContourVertexNode.Value;
		
			// Add the ear triangle to our triangles list
			a_rTrianglesList.Add( iNextAdjacentContourVertexIndex );
			a_rTrianglesList.Add( iEarTipVertexIndex );
			a_rTrianglesList.Add( iPreviousAjdacentContourVertexIndex );

			// Remove the ear tip from vertices / convex / ear lists
			oVertexIndexesList.Remove( iEarTipVertexIndex );
			rConvexVertexIndexesList.Remove( iEarTipVertexIndex );

			// Adjacent n-1 vertex
			// if was convex => remain convex, can possibly an ear
			// if was an ear => can possibly not remain an ear
			// if was reflex => can possibly convex and possibly an ear
			if( rReflexVertexIndexesList.Contains( iPreviousAjdacentContourVertexIndex ) == true )
			{
				CircularLinkedListNode<int> rPreviousPreviousAdjacentContourVertexNode = rPreviousAdjacentContourVertexNode.Previous;

				Vector3 f3AdjacentContourVertex         = a_rVerticesArray[ rPreviousAdjacentContourVertexNode.Value ];
				Vector3 f3PreviousAdjacentContourVertex = a_rVerticesArray[ rPreviousPreviousAdjacentContourVertexNode.Value ];
				Vector3 f3NextAdjacentContourVertex     = a_rVerticesArray[ rPreviousAdjacentContourVertexNode.Next.Value ];

				if( IsReflexVertex( f3AdjacentContourVertex, f3PreviousAdjacentContourVertex, f3NextAdjacentContourVertex ) == false )
				{
					rReflexVertexIndexesList.Remove( iPreviousAjdacentContourVertexIndex );
					rConvexVertexIndexesList.AddFirst( iPreviousAjdacentContourVertexIndex );
				}
			}

			// Adjacent n+1 vertex
			// if was convex => remain convex, can possibly an ear
			// if was an ear => can possibly not remain an ear
			// if was reflex => can possibly convex and possibly an ear
			if( rReflexVertexIndexesList.Contains( iNextAdjacentContourVertexIndex ) == true )
			{
				CircularLinkedListNode<int> rNextNextAdjacentContourVertexNode = rNextAdjacentContourVertexNode.Next;

				Vector3 f3AdjacentContourVertex         = a_rVerticesArray[ rNextAdjacentContourVertexNode.Value ];
				Vector3 f3PreviousAdjacentContourVertex = a_rVerticesArray[ rNextAdjacentContourVertexNode.Previous.Value ];
				Vector3 f3NextAdjacentContourVertex     = a_rVerticesArray[ rNextNextAdjacentContourVertexNode.Value ];

				if( IsReflexVertex( f3AdjacentContourVertex, f3PreviousAdjacentContourVertex, f3NextAdjacentContourVertex ) == false )
				{
					rReflexVertexIndexesList.Remove( iNextAdjacentContourVertexIndex );
					rConvexVertexIndexesList.AddFirst( iNextAdjacentContourVertexIndex );
				}
			}

			if( rConvexVertexIndexesList.Contains( iPreviousAjdacentContourVertexIndex ) == true )
			{
				if( IsEarTip( a_rVerticesArray, iPreviousAjdacentContourVertexIndex, oVertexIndexesList, rReflexVertexIndexesList ) == true )
				{
					if( rEarTipVertexIndexesList.Contains( iPreviousAjdacentContourVertexIndex ) == false )
					{
						rEarTipVertexIndexesList.AddLast( iPreviousAjdacentContourVertexIndex );
					}
				}
				else
				{
					rEarTipVertexIndexesList.Remove( iPreviousAjdacentContourVertexIndex );
				}
			}

			if( rConvexVertexIndexesList.Contains( iNextAdjacentContourVertexIndex ) == true )
			{
				if( IsEarTip( a_rVerticesArray, iNextAdjacentContourVertexIndex, oVertexIndexesList, rReflexVertexIndexesList ) == true )
				{
					if( rEarTipVertexIndexesList.Contains( iNextAdjacentContourVertexIndex ) == false )
					{
						rEarTipVertexIndexesList.AddFirst( iNextAdjacentContourVertexIndex );
					}
				}
				else
				{
					rEarTipVertexIndexesList.Remove( iNextAdjacentContourVertexIndex );
				}
			}

			rEarTipVertexIndexesList.Remove( iEarTipVertexIndex );
		}

		// Rescale vertices
		for( iVertexIndex = 0; iVertexIndex < iVerticesCount; ++iVertexIndex )
		{
			Vector3 f3VertexPos = a_rVerticesArray[ iVertexIndex ];
			f3VertexPos = ( f3VertexPos - a_f3PivotPoint ) * a_fScale;
			a_rVerticesArray[ iVertexIndex ] = f3VertexPos;
		}
	}

	// Build indexes lists of reflex and convex vertex
	private static void BuildReflexConvexVertexIndexesLists( Vector3[ ] a_rContourVerticesArray, CircularLinkedList<int> a_rContourVertexIndexesList, out LinkedList<int> a_rReflexVertexIndexesList, out LinkedList<int> a_rConvexVertexIndexesList )
	{
		LinkedList<int> oReflexVertexIndexesList = new LinkedList<int>( );
		LinkedList<int> oConvexVertexIndexesList = new LinkedList<int>( );

		// Iterate contour vertices
		CircularLinkedListNode<int> rContourNode = a_rContourVertexIndexesList.First;
		do
		{
			int iContourVertexIndex         = rContourNode.Value;
			Vector3 f3ContourVertex         = a_rContourVerticesArray[ iContourVertexIndex ];
			Vector3 f3PreviousContourVertex = a_rContourVerticesArray[ rContourNode.Previous.Value ];
			Vector3 f3NextContourVertex     = a_rContourVerticesArray[ rContourNode.Next.Value ];

			// Sorting reflex / convex vertices
			// Reflex vertex forms a triangle with an angle >= 180°
			if( IsReflexVertex( f3ContourVertex, f3PreviousContourVertex, f3NextContourVertex ) == true )
			{
				oReflexVertexIndexesList.AddLast( iContourVertexIndex );
			}
			else // angle < 180° => Convex vertex
			{
				oConvexVertexIndexesList.AddLast( iContourVertexIndex );
			}

			rContourNode = rContourNode.Next;
		}
		while( rContourNode != a_rContourVertexIndexesList.First );

		// Transmit results
		a_rReflexVertexIndexesList = oReflexVertexIndexesList;
		a_rConvexVertexIndexesList = oConvexVertexIndexesList;
	}

	// Find a pair of inner contour vertex / outer contour vertex that are mutually visible
	public static Contour InsertInnerContourIntoOuterContour( Contour a_rOuterContour, Contour a_rInnerContour )
	{
		// Look for the inner vertex of maximum x-value
		Vector2 f2InnerContourVertexMax = Vector2.one * int.MinValue;
		CircularLinkedListNode<Vector2> rMutualVisibleInnerContourVertexNode = null;

		CircularLinkedList<Vector2> rInnerContourVertexList = a_rInnerContour.Vertices;
		CircularLinkedListNode<Vector2> rInnerContourVertexNode = rInnerContourVertexList.First;
		
		do
		{
			// x-value
			Vector2 f2InnerContourVertex = rInnerContourVertexNode.Value;

			// New max x found
			if( f2InnerContourVertexMax.x < f2InnerContourVertex.x )
			{
				f2InnerContourVertexMax = f2InnerContourVertex;
				rMutualVisibleInnerContourVertexNode = rInnerContourVertexNode;
			}

			// Go to next vertex
			rInnerContourVertexNode = rInnerContourVertexNode.Next;
		}
		while( rInnerContourVertexNode != rInnerContourVertexList.First );

		// Visibility ray
		Ray oInnerVertexVisibilityRay = new Ray( f2InnerContourVertexMax, Vector3.right );
		float fClosestDistance = int.MaxValue;
		Vector2 f2ClosestOuterEdgeStart = Vector2.zero;
		Vector2 f2ClosestOuterEdgeEnd = Vector2.zero;

		Contour rOuterCutContour = new Contour( a_rOuterContour.Region );
		rOuterCutContour.AddLast( a_rOuterContour.Vertices );

		CircularLinkedList<Vector2> rOuterCutContourVertexList = rOuterCutContour.Vertices;
		CircularLinkedListNode<Vector2> rOuterContourVertexEdgeStart = null;

		// Raycast from the inner contour vertex to every edge
		CircularLinkedListNode<Vector2> rOuterContourVertexNode = rOuterCutContourVertexList.First;
		do
		{
			// Construct outer edge from current and next outer contour vertices
			Vector2 f2OuterEdgeStart = rOuterContourVertexNode.Value;
			Vector2 f2OuterEdgeEnd = rOuterContourVertexNode.Next.Value;
			Vector2 f2OuterEdge = f2OuterEdgeEnd - f2OuterEdgeStart;

			// Orthogonal vector to edge (pointing to polygon interior)
			Vector2 f2OuterEdgeNormal = Uni2DUtilsMath.PerpVector2( f2OuterEdge );

			// Vector from edge start to inner vertex
			Vector2 f2OuterEdgeStartToInnerVertex = f2InnerContourVertexMax - f2OuterEdgeStart;

			// If the inner vertex is on the left of the edge (interior),
			// test if there's any intersection
			if( Vector2.Dot( f2OuterEdgeStartToInnerVertex, f2OuterEdgeNormal ) >= 0.0f )
			{
				float fDistanceT;

				// If visibility intersects outer edge... 
				if( Uni2DUtilsMath.Raycast2DSegment( oInnerVertexVisibilityRay, f2OuterEdgeStart, f2OuterEdgeEnd, out fDistanceT ) == true )
				{
					// Is it the closest intersection we found?
					if( fClosestDistance > fDistanceT )
					{
						fClosestDistance = fDistanceT;
						rOuterContourVertexEdgeStart = rOuterContourVertexNode;

						f2ClosestOuterEdgeStart = f2OuterEdgeStart;
						f2ClosestOuterEdgeEnd = f2OuterEdgeEnd;
					}
				}
			}

			// Go to next edge
			rOuterContourVertexNode = rOuterContourVertexNode.Next;
		}
		while( rOuterContourVertexNode != rOuterCutContourVertexList.First );

		// Take the vertex of maximum x-value from the closest intersected edge
		Vector2 f2ClosestVisibleOuterContourVertex;
		CircularLinkedListNode<Vector2> rMutualVisibleOuterContourVertexNode;
		if( f2ClosestOuterEdgeStart.x < f2ClosestOuterEdgeEnd.x )
		{
			f2ClosestVisibleOuterContourVertex = f2ClosestOuterEdgeEnd;
			rMutualVisibleOuterContourVertexNode = rOuterContourVertexEdgeStart.Next;
		}
		else
		{
			f2ClosestVisibleOuterContourVertex = f2ClosestOuterEdgeStart;
			rMutualVisibleOuterContourVertexNode = rOuterContourVertexEdgeStart;
		}

		// Looking for points inside the triangle defined by inner vertex, intersection point an closest outer vertex
		// If a point is inside this triangle, at least one is a reflex vertex
		// The closest reflex vertex which minimises the angle this-vertex/inner vertex/intersection vertex
		// would be choosen as the mutual visible vertex
		Vector3 f3IntersectionPoint = oInnerVertexVisibilityRay.GetPoint( fClosestDistance );
		Vector2 f2InnerContourVertexToIntersectionPoint = new Vector2( f3IntersectionPoint.x, f3IntersectionPoint.y ) - f2InnerContourVertexMax;
		Vector2 f2NormalizedInnerContourVertexToIntersectionPoint = f2InnerContourVertexToIntersectionPoint.normalized;

		float fMaxDotAngle = float.MinValue;
		float fMinDistance = float.MaxValue;
		rOuterContourVertexNode = rOuterCutContourVertexList.First;
		do
		{
			Vector2 f2OuterContourVertex = rOuterContourVertexNode.Value;

			// if vertex not part of triangle
			if( f2OuterContourVertex != f2ClosestVisibleOuterContourVertex )
			{
				// if vertex is inside triangle...
				if( Uni2DUtilsMath.IsPointInsideTriangle( f2InnerContourVertexMax, f3IntersectionPoint, f2ClosestVisibleOuterContourVertex, f2OuterContourVertex ) == true )
				{
					// if vertex is reflex
					Vector2 f2PreviousOuterContourVertex = rOuterContourVertexNode.Previous.Value;
					Vector2 f2NextOuterContourVertex = rOuterContourVertexNode.Next.Value;
	
					if( IsReflexVertex( f2OuterContourVertex, f2PreviousOuterContourVertex, f2NextOuterContourVertex ) == true )
					{
						// Use dot product as distance
						Vector2 f2InnerContourVertexToReflexVertex = f2OuterContourVertex - f2InnerContourVertexMax;

						// INFO: f2NormalizedInnerContourVertexToIntersectionPoint == Vector3.right (if everything is right)
						float fDistance = Vector2.Dot( f2NormalizedInnerContourVertexToIntersectionPoint, f2InnerContourVertexToReflexVertex );
						float fDotAngle = Vector2.Dot( f2NormalizedInnerContourVertexToIntersectionPoint, f2InnerContourVertexToReflexVertex.normalized );

						// New mutual visible vertex if angle smaller (e.g. dot angle larger) than min or equal and closer
						if( fDotAngle > fMaxDotAngle || ( fDotAngle == fMaxDotAngle && fDistance < fMinDistance ) )
						{
							fMaxDotAngle = fDotAngle;
							fMinDistance = fDistance;
							rMutualVisibleOuterContourVertexNode = rOuterContourVertexNode;
						}
					}
				}
			}

			// Go to next vertex
			rOuterContourVertexNode = rOuterContourVertexNode.Next;
		}
		while( rOuterContourVertexNode != rOuterCutContourVertexList.First );

		// Insert now the cut into the polygon
		// The cut starts from the outer contour mutual visible vertex to the inner vertex
		CircularLinkedListNode<Vector2> rOuterContourVertexNodeToInsertBefore = rMutualVisibleOuterContourVertexNode.Next;

		// Loop over the inner contour starting from the inner contour vertex...
		rInnerContourVertexNode = rMutualVisibleInnerContourVertexNode;
		do
		{
			// ... add the inner contour vertex before the outer contour vertex after the cut
			rOuterCutContourVertexList.AddBefore( rOuterContourVertexNodeToInsertBefore, rInnerContourVertexNode.Value );
			rInnerContourVertexNode = rInnerContourVertexNode.Next;
		}
		while( rInnerContourVertexNode != rMutualVisibleInnerContourVertexNode );

		// Close the cut by doubling the inner and outer contour vertices
		rOuterCutContourVertexList.AddBefore( rOuterContourVertexNodeToInsertBefore, rMutualVisibleInnerContourVertexNode.Value );
		rOuterCutContourVertexList.AddBefore( rOuterContourVertexNodeToInsertBefore, rMutualVisibleOuterContourVertexNode.Value );

		return rOuterCutContour;
	}

	// Return true if the vertex I is a reflex vertex, i.e. the angle JÎH is >= 180°
	private static bool IsReflexVertex( Vector3 a_f3VertexI, Vector3 a_f3VertexJ, Vector3 a_f3VertexH )
	{
		Vector3 f3SegmentJI = a_f3VertexI - a_f3VertexJ;
		Vector3 f3SegmentIH = a_f3VertexH - a_f3VertexI;
		Vector3 f3JINormal  = new Vector3( f3SegmentJI.y, - f3SegmentJI.x, 0 );

		return Vector3.Dot( f3SegmentIH, f3JINormal ) < 0.0f;
	}

	// Build and return a list of vertex indexes that are ear tips.
	private static CircularLinkedList<int> BuildEarTipVerticesList( Vector3[ ] a_rMeshVertices, CircularLinkedList<int> a_rOuterContourVertexIndexesList, LinkedList<int> a_rReflexVertexIndexesList, LinkedList<int> a_rConvexVertexIndexesList )
	{
		CircularLinkedList<int> oEarTipVertexIndexesList = new CircularLinkedList<int>( );

		// Iterate convex vertices
		for( LinkedListNode<int> rConvexIndexNode = a_rConvexVertexIndexesList.First; rConvexIndexNode != null; rConvexIndexNode = rConvexIndexNode.Next )
		{
			// The convex vertex index
			int iConvexContourVertexIndex = rConvexIndexNode.Value;

			// Is the convex vertex is an ear tip?
			if( IsEarTip( a_rMeshVertices, iConvexContourVertexIndex, a_rOuterContourVertexIndexesList, a_rReflexVertexIndexesList ) == true )
			{
				// Yes: adds it to the list
				oEarTipVertexIndexesList.AddLast( iConvexContourVertexIndex );
			}
		}

		// Return the ear tip list
		return oEarTipVertexIndexesList;
	}

	// Return true if the specified convex vertex is an ear tip
	private static bool IsEarTip( Vector3[ ] a_rMeshVertices, int a_iEarTipConvexVertexIndex, CircularLinkedList<int> a_rContourVertexIndexesList, LinkedList<int> a_rReflexVertexIndexesList )
	{
		CircularLinkedListNode<int> rContourVertexNode = a_rContourVertexIndexesList.Find( a_iEarTipConvexVertexIndex );

		int iPreviousContourVertexIndex = rContourVertexNode.Previous.Value;
		int iNextContourVertexIndex = rContourVertexNode.Next.Value;

		// Retrieve previous (i-1) / current (i) / next (i+1) vertices to form the triangle < Vi-1, Vi, Vi+1 >
		Vector3 f3ConvexContourVertex   = a_rMeshVertices[ a_iEarTipConvexVertexIndex ];
		Vector3 f3PreviousContourVertex = a_rMeshVertices[ iPreviousContourVertexIndex ];
		Vector3 f3NextContourVertex     = a_rMeshVertices[ iNextContourVertexIndex ];

		// Look for an inner point into the triangle formed by the 3 vertices
		// Only need to look over the reflex vertices.
		for( LinkedListNode<int> rReflexIndexNode = a_rReflexVertexIndexesList.First; rReflexIndexNode != null; rReflexIndexNode = rReflexIndexNode.Next )
		{
			// Retrieve the reflex vertex
			int iReflexContourVertexIndex = rReflexIndexNode.Value;

			// Is the point inside the triangle?
			// (Exclude the triangle points themselves)
			Vector3 f3ReflexContourVertex = a_rMeshVertices[ iReflexContourVertexIndex ];
			if( f3ReflexContourVertex != f3PreviousContourVertex && f3ReflexContourVertex != f3ConvexContourVertex && f3ReflexContourVertex != f3NextContourVertex )
			{
				if( Uni2DUtilsMath.IsPointInsideTriangle( f3PreviousContourVertex, f3ConvexContourVertex, f3NextContourVertex, f3ReflexContourVertex ) == true )
				{
					// Point is inside triangle: not an ear tip
					return false;
				}
			}
		}

		// No point inside the triangle: ear tip found!
		return true;
	}

	// Build a circular linked list with a_iIndexesCount index, from 0 to a_iIndexesCount - 1
	private static CircularLinkedList<int> BuildContourVertexIndexesList( int a_iIndexesCount, int a_iIndexOffset )
	{
		CircularLinkedList<int> oVertexIndexesList = new CircularLinkedList<int>( );

		for( int iIndex = 0; iIndex < a_iIndexesCount; ++iIndex )
		{
			oVertexIndexesList.AddLast( iIndex + a_iIndexesCount );
		}

		return oVertexIndexesList;
	}

	private static List<Mesh> BuildExtrudedTrianglesFromPolygonizedContours( Vector3[ ] a_rMeshVerticesArray, List<int> a_rContourTrianglesListArray, float a_fExtrusionDepth )
	{
		int iTriangleCount = a_rContourTrianglesListArray.Count;
		Vector3[ ] oTriangleVertices = new Vector3[ 3 ];
		List<int> oTrianglesList = new List<int>( 3 );
		List<Mesh> oTriangleMeshesList = new List<Mesh>( iTriangleCount / 3 );

		oTrianglesList.Add( 0 );
		oTrianglesList.Add( 1 );
		oTrianglesList.Add( 2 );

		for( int iTriangleIndex = 0; iTriangleIndex < iTriangleCount; iTriangleIndex += 3 )
		{
			int iTriangleVertexIndexA = a_rContourTrianglesListArray[ iTriangleIndex     ];
			int iTriangleVertexIndexB = a_rContourTrianglesListArray[ iTriangleIndex + 1 ];
			int iTriangleVertexIndexC = a_rContourTrianglesListArray[ iTriangleIndex + 2 ];

			oTriangleVertices[ 0 ] = a_rMeshVerticesArray[ iTriangleVertexIndexA ];
			oTriangleVertices[ 1 ] = a_rMeshVerticesArray[ iTriangleVertexIndexB ];
			oTriangleVertices[ 2 ] = a_rMeshVerticesArray[ iTriangleVertexIndexC ];

			Mesh rTriangleMesh = BuildExtrudedMeshFromPolygonizedContours( oTriangleVertices, oTrianglesList, a_fExtrusionDepth );
			oTriangleMeshesList.Add( rTriangleMesh );
		}

		return oTriangleMeshesList;
	}

	private static Mesh BuildExtrudedMeshFromPolygonizedContours( Vector3[ ] a_rMeshVerticesArray, List<int> a_rContourTrianglesListsArray, float a_fExtrusionDepth )
	{
		// Copy mesh vertices
		int iVerticesCount = a_rMeshVerticesArray.Length;
		int iExtrudedVerticesCount = 2 * iVerticesCount;
		Vector3[ ] oExtrudedVerticesArray = new Vector3[ iExtrudedVerticesCount ];
		//a_rMeshVerticesArray.CopyTo( oExtrudedVerticesArray, 0 );

		float fHalfExtrusionDepth = a_fExtrusionDepth * 0.5f;

		// Build extruded vertices
		for( int iVertexIndex = 0; iVertexIndex < iVerticesCount; ++iVertexIndex )
		{
			Vector3 f3Vertex = a_rMeshVerticesArray[ iVertexIndex ];

			oExtrudedVerticesArray[ iVertexIndex ] = f3Vertex - fHalfExtrusionDepth * Vector3.back;
			oExtrudedVerticesArray[ iVertexIndex + iVerticesCount ] = f3Vertex + fHalfExtrusionDepth * Vector3.back;
		}

		// Copy mesh triangles
		List<int> oExtrudedTrianglesList = new List<int>( );
		int iTrianglesCount = a_rContourTrianglesListsArray.Count;
		oExtrudedTrianglesList.AddRange( a_rContourTrianglesListsArray );

		// Build extruded triangles
		// CW to CCW
		for( int iOriginalTriangleVertexIndex = 0; iOriginalTriangleVertexIndex < iTrianglesCount; iOriginalTriangleVertexIndex += 3 )
		{				
			oExtrudedTrianglesList.Add( iVerticesCount + a_rContourTrianglesListsArray[ iOriginalTriangleVertexIndex + 1 ] );
			oExtrudedTrianglesList.Add( iVerticesCount + a_rContourTrianglesListsArray[ iOriginalTriangleVertexIndex     ] );
			oExtrudedTrianglesList.Add( iVerticesCount + a_rContourTrianglesListsArray[ iOriginalTriangleVertexIndex + 2 ] );
		}

		// Build jointure band
		for( int iContourTriangleVertexIndex = 0; iContourTriangleVertexIndex < iVerticesCount; ++iContourTriangleVertexIndex )
		{
			int iNextContourTriangleVertexIndex = ( iContourTriangleVertexIndex + 1 ) % iVerticesCount;

			oExtrudedTrianglesList.Add( iContourTriangleVertexIndex );	// nA
			oExtrudedTrianglesList.Add( iNextContourTriangleVertexIndex );	// nA+1
			oExtrudedTrianglesList.Add( iContourTriangleVertexIndex + iVerticesCount );	// nB

			oExtrudedTrianglesList.Add( iNextContourTriangleVertexIndex );	// nA+1
			oExtrudedTrianglesList.Add( iNextContourTriangleVertexIndex + iVerticesCount );	// nB+1
			oExtrudedTrianglesList.Add( iContourTriangleVertexIndex + iVerticesCount );	// nB				
		}

		// Build mesh
		Mesh oExtrudedMesh = new Mesh( );
		oExtrudedMesh.vertices  = oExtrudedVerticesArray;
		oExtrudedMesh.uv        = new Vector2[ iExtrudedVerticesCount ];	// Dummy UVs to prevent info messages...
		oExtrudedMesh.triangles = oExtrudedTrianglesList.ToArray();

		return oExtrudedMesh;
	}
}
#endif