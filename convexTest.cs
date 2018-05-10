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
		showdebugresult();
		selectExtremum();
		FindTriangle();
		
		Debug.Break();}
			
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
		Debug.DrawLine(forward,back,color);}
		
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
		Debug.DrawLine(extremumlist[bx],extremumlist[by],Color.yellow);}
		
	void FindTriangle(){
		Vector3 furthest = Vector3.zero, projected = Vector3.zero;
		foreach (Vector3 v in this.SupportVert){
			Vector3 p = Vector3.Project(convexPoints[0]-v,(convexPoints[0]-convexPoints[1]).normalized);
			Vector3 length = p-v;
			if (length.sqrMagnitude > furthest.sqrMagnitude) {furthest = length;projected = p;}
		}
		Debug.DrawLine(projected,furthest,Color.red); print(0);

	}
		
}


