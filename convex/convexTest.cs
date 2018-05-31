using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class convexTest : MonoBehaviour {
	Vector3[] vertlist;
	Vector3 centroid;
	ArrayList discard;
	List<Vector3> SupportVert;
	List<Vector3> convexPoints;
	Vector3
		minX,maxX,
		minY,maxY,
		minZ,maxZ;
	Mesh convexMesh;
	mesh2Hedges hull;
	
	//*********************************************************
	void Start () {
		this.vertlist = new Vector3[100];
		this.discard = new ArrayList();
		this.SupportVert = new List<Vector3>();
		this.convexPoints = new List<Vector3>();
		for(int i =0; i< this.vertlist.Length;i++){
			this.vertlist[i] = Random.insideUnitSphere;
			this.SupportVert.Add( this.vertlist[i]);}
		findSupportVert();
		FindExtremum();
		//showdebugresult();
		selectExtremum();
		FindTriangle();
		FindTetra();
		//drawDebugTetra();
		buildMesh();
		toHalfEdge();
		
		Debug.Break();}
		
	//*********************************************************

	void findSupportVert () {
		//compute centroid position
		foreach (Vector3 v in this.vertlist){this.centroid += v;}
		this.centroid /= this.vertlist.Length;
		//initialize loop
		Vector3 support = new Vector3();
		float max = float.Epsilon;
		//iterate points to get the support point
		foreach (Vector3 v in this.vertlist){
			Vector3 normalDirection =(v - this.centroid).normalized;//vector= end - start
			//check all other points to find support
			max = float.Epsilon;
			foreach (Vector3 t in this.SupportVert){
				Vector3 compared= t - this.centroid;
			    float dot = Vector3.Dot (compared,normalDirection);
				if (dot > max) {
				    max = dot;
				    support = t;}}
			//after iteration, if current is not support, discard
			if (support != v) {this.discard.Add (v);
				this.SupportVert.Remove(v);}}}
		
	void FindExtremum(){
		this.minX = this.SupportVert[0]; this.maxX = this.SupportVert[0];
		this.minY = this.SupportVert[0]; this.maxY = this.SupportVert[0];
		this.minZ = this.SupportVert[0]; this.maxZ = this.SupportVert[0];
		foreach (Vector3 v in this.SupportVert){
			if (v.x > this.maxX.x) this.maxX = v; if (v.x < this.minX.x) this.minX = v;
			if (v.y > this.maxY.y) this.maxY = v; if (v.y < this.minY.y) this.minY = v;
			if (v.z > this.maxZ.z) this.maxZ = v; if (v.z < this.minZ.z) this.minZ = v;}}
		
	void showdebugresult(){
		foreach (Vector3 v in this.SupportVert){
			//Debug.DrawLine(this.centroid, v, Color.white);
		}
		foreach (Vector3 v in this.discard){
			//Debug.DrawLine(this.centroid, v, Color.red);
		}
		DrawDebugCube(this.minX,Color.cyan); DrawDebugCube(this.maxX,Color.yellow);
		DrawDebugCube(this.minY,Color.black);DrawDebugCube(this.maxY,Color.gray);
		DrawDebugCube(this.minZ,Color.green);DrawDebugCube(this.maxZ,Color.blue);}
		
	void DrawDebugCube(Vector3 pos, Color color){
		float size = 0.1f;
		Vector3 up		= pos + Vector3.up		*size;	Vector3 down	= pos + Vector3.down	*size;
		Vector3 right	= pos + Vector3.right	*size;	Vector3 left	= pos + Vector3.left	*size;
		Vector3 forward	= pos + Vector3.forward	*size;	Vector3 back	= pos + Vector3.back	*size;
		Debug.DrawLine(up,down,		color);
		Debug.DrawLine(right,left,	color);
		Debug.DrawLine(forward,back,color);
	}
		
	void selectExtremum(){
		Vector3[] extremumlist = new Vector3[6];
		Vector3[,] size = new Vector3[6,6];
		Vector3 biggest = Vector3.zero;int bx=0,by=0;
		extremumlist[0] = this.minX; extremumlist[1] = this.maxX;
		extremumlist[2] = this.minY; extremumlist[3] = this.maxY;
		extremumlist[4] = this.minZ; extremumlist[5] = this.maxZ;
		//loop all size combination for curiosity, should just take each extrem pair only really
		for (int i = 0; i < extremumlist.Length; i++) {
			for (int j = 0; j <  extremumlist.Length; j++) {
				if (extremumlist[i] == extremumlist[j]) continue;
				size[i,j] = extremumlist[i]- extremumlist[j];}}
		//loop to find the biggest
		for (int i = 0; i < size.Length; i++) {
			int ax = i/6, ay = i%6;
			if (ax == ay) continue;
			if (size[ax,ay].sqrMagnitude > biggest.sqrMagnitude){ biggest = size[ax,ay]; bx = ax; by = ay;}}
		this.convexPoints.Add(extremumlist[bx]);
		this.convexPoints.Add(extremumlist[by]);
		//Debug.DrawLine(extremumlist[bx],extremumlist[by],Color.yellow);
	}
		
	void FindTriangle(){
		Vector3 furthest = convexPoints[0], projected = Vector3.zero;
		float maxlength = 0;
		foreach (Vector3 v in this.SupportVert){//v is point to measure distance to
			Vector3 ov = v-convexPoints[0];//vector to the point to project
			Vector3 o1 = convexPoints[1]-convexPoints[0];//segment to project onto
			Vector3 p = Vector3.Project(ov,o1.normalized);//resulting projection point on the segment
			Vector3 length = v - p;//length = v-0 + p+o
			float l = length.sqrMagnitude;//should concatenated with above
			if (l > maxlength) {furthest = v;projected = p;maxlength = l;}
			//DrawDebugCube(v,Color.gray);
			//DrawDebugCube(convexPoints[0]+p,Color.black);
			//Debug.DrawLine(convexPoints[0]+p,v,Color.gray);
		}
		this.convexPoints.Add(furthest);
		//Debug.DrawLine(convexPoints[0]+projected,furthest,Color.red);
		//DrawDebugCube(convexPoints[0]+projected,Color.green);
	}
		
	void FindTetra(){
		Vector3 furthest = convexPoints[0], projected = Vector3.zero;
		float maxlength = 0;
		Vector3 o1 = convexPoints[1]-convexPoints[0];
		Vector3 o2 = convexPoints[2]-convexPoints[0];
		Vector3 ox = Vector3.Cross(o1,o2);
		foreach (Vector3 v in this.SupportVert){
			Vector3 ov = v-convexPoints[0];		
			Vector3 p = Vector3.ProjectOnPlane(ov,ox);
			Vector3 length = v -(convexPoints[0] + p);
			float l = length.sqrMagnitude;//print(l);
			if (l > maxlength) {furthest = v;projected = p;maxlength = l;}
			//DrawDebugCube(v,Color.gray);
			//DrawDebugCube(convexPoints[0]+p,Color.black);
			//Debug.DrawLine(convexPoints[0]+p,v,Color.gray);
		}
		//print("max: " + maxlength);
		this.convexPoints.Add(furthest);
		//Debug.DrawLine(convexPoints[0],convexPoints[1],Color.yellow);
		//Debug.DrawLine(convexPoints[0],convexPoints[2],Color.yellow);
		//Debug.DrawLine(convexPoints[1],convexPoints[2],Color.yellow);
		//Debug.DrawLine(convexPoints[0]+projected,furthest,Color.red);
		//DrawDebugCube(convexPoints[0]+projected,Color.green);
	}
	
	void drawDebugTetra(){
		Debug.DrawLine(convexPoints[0],convexPoints[1],Color.yellow);
		Debug.DrawLine(convexPoints[0],convexPoints[2],Color.yellow);
		Debug.DrawLine(convexPoints[1],convexPoints[2],Color.yellow);
		Debug.DrawLine(convexPoints[0],convexPoints[3],Color.green);
		Debug.DrawLine(convexPoints[3],convexPoints[2],Color.green);
		Debug.DrawLine(convexPoints[1],convexPoints[3],Color.green);}
	
	void buildMesh(){
		this.convexMesh = new Mesh();
		this.convexMesh.vertices = convexPoints.ToArray();		
		//first find cross product of triangle
		Vector3 o1 = convexPoints[1]-convexPoints[0];
		Vector3 o2 = convexPoints[2]-convexPoints[0];
		Vector3 o3 = convexPoints[3]-convexPoints[0];
		Vector3 ox = Vector3.Cross(o1,o2);
		//check if 4th point is on the same side
		Vector3 p = Vector3.ProjectOnPlane(o3,ox);
		Vector3 p3 = convexPoints[3]-(convexPoints[0] + p);
		float side = Vector3.Dot(ox.normalized,p3.normalized);
		//Debug.DrawLine((convexPoints[0] + p),convexPoints[3],Color.red);
		if (side>0){		//yes -> reverse triangle winding
			this.convexMesh.triangles = new int[]{
				1,0,2,
				3,0,1,
				3,2,0,
				3,1,2};}
		else{				//no -> triangle winding
			this.convexMesh.triangles = new int[]{
				0,1,2,
				0,3,1,
				2,3,0,
				1,3,2};}
		if(side == 0) Debug.LogError("degenerate mesh");
		convexMesh.RecalculateNormals();
		this.gameObject.AddComponent<MeshFilter>();
		this.gameObject.GetComponent<MeshFilter>().mesh = convexMesh;
		Material newMaterial = new Material(Shader.Find("Diffuse"));
		this.gameObject.AddComponent<MeshRenderer>();
		this.gameObject.GetComponent<MeshRenderer>().material = newMaterial;}
		
	void toHalfEdge(){
		this.hull = new mesh2Hedges();
		this.hull.UnityMesh = convexMesh;
		this.hull.setup();
		this.hull.DrawDebugHedge();}
	
	class conflictList{
		public mesh2Hedges.half.face triangle;
		public List<Vector3> conflictPoints;
		conflictList(){
			triangle = new mesh2Hedges.half.face();
			conflictPoints = new List<Vector3>();}}
		
	void buildConvex(){
		List<conflictList> conflicts = new List<conflictList>();
		
		//for each face in hull.hface create a conflict list for that face and put it in conflicts
		//->for each point in convexpoint
		//--> for each triangle in hface
		//--> find the facing of the pointo to the face
		//--> if facing: find the distance of the point to face
		//--> if closest face, set face to closest
		
		//-> if cloest face == null, set point to discard list
		//-> else set point to conflict list of closest face
		
		
		
		//for each conflict list
		//-> for each point in conflict list --- while conflict list length > 0
		//--> find distance to face
		//--> if distance bigger than furthest, new point = furthest
		
		//-> for each face in hull
		//--> test furthest facing
		//--> if facing then face set face to seen list
		
		//-> find the horizon of the conflict face
		//--> for each edge of the triangle
		//---> find the next face
		//---> if next face is in process, go next edge, else set next face in processed list
		//---> if next face is in visible, find next face (recursion)
		//---> if next face is not visible, set next face edge in horizon, go next edge
		
		//-> for each edge in horizon
		//--> build triangle with furthest
		
		//-> create new conflict list from point in this conflict list for the new triangle
		//-> delete triangle of the conflict list
		//-> delete the conflict list
		
		//?? no stopping mechanism? check when conflict list is empty ** check when list of conflict is empty

		
		
		
		
	}
	//*********************** utils *************************
	float TakePointSqrMagnitude2segment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd){
		Vector3 ov = point-segmentStart;//vector to the point to project
		Vector3 o1 = segmentEnd-segmentStart;//segment to project onto
		Vector3 p = Vector3.Project(ov,o1.normalized);//resulting projection point on the segment
		return (point - p).sqrMagnitude;//length = v-0 + p+o
	}
	float TakepointSqrMagnitude2triangle(Vector3 point,Vector3 p0, Vector3 p1, Vector3 p2){
		Vector3 o1 = p1-p0;
		Vector3 o2 = p2-p0;
		Vector3 ox = Vector3.Cross(o1,o2);
		Vector3 ov = point-p0;		
		Vector3 p = Vector3.ProjectOnPlane(ov,ox);
		return (point -(p0 + p)).sqrMagnitude;
	}
}


