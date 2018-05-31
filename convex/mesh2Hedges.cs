using UnityEngine;
using System.Collections;

public class mesh2Hedges {
	
	//public:
	// unitymesh:mesh
	// half.edge/face/vert container
	// half class
	// triangle/medge
	// fn:setup
	// fn:drawdebug edge/twin/solo/clone -> (latter not implemented)
	
	public Mesh UnityMesh;
	
	//extra structure for manipulating data
	triangle[] trianglelist;
	medge[] edgelist;
	
	//half edge container
	public half.edge[] hedge;
	public half.face[] hface;
	public half.vert[] hvert;
	
	//data for reconstruction
	twin[] twinlist;
	twin[] clonelist;
	medge[] sololist;
	medge[] singlelist;
		
	public class triangle{
		public int index1 = 0,index2 = 0, index3 = 0;
		public Vector3 pos1, pos2, pos3, barycentre;}
	
	public class medge{
		//mesh edge for reference
		public int index1, index2;
		public Vector3 pos1,pos2;}
	
	private class twin{
		public medge n1,n2;}

	public class half{
		public class face{
			public half.edge edge;
			public medge medge;
			public triangle triangle;}
		public class edge{
			public medge currentmedge;
			public half.vert endvert;
			public half.edge twinedge;
			public medge twinmedge;
			public half.face face;
			public half.edge nexthalfedge;
			public medge nextmedge;}
		public class vert{
			public Vector3 position;
			public half.edge edge;
			public medge medge;}}
		
	void CreateTriangleFromMesh(){
		//turn unity flat list into separate complete triangle data
		Mesh m = UnityMesh;
		trianglelist = new triangle[m.triangles.Length/3];
		int i = 0;while (i < trianglelist.Length){
			int x = i*3;
			trianglelist[i] = new triangle();
			trianglelist[i].index1 = m.triangles[x];
			trianglelist[i].index2 = m.triangles[x+1];
			trianglelist[i].index3 = m.triangles[x+2];
			trianglelist[i].pos1 = m.vertices[trianglelist[i].index1];
			trianglelist[i].pos2 = m.vertices[trianglelist[i].index2];
			trianglelist[i].pos3 = m.vertices[trianglelist[i].index3];
			i++;}}
	
	void CreateMeshEdge(){
		//go through triangle to get their edge loop
		//use those edge loop to initialize first structure of half edge
		//can't initialize twin edge yet
		edgelist = new medge[trianglelist.Length*3];
		hface = new half.face[trianglelist.Length];
		hedge = new half.edge[edgelist.Length];
		hvert = new half.vert[edgelist.Length];
		int i = 0; while (i < trianglelist.Length){
			int x = i*3;
			//put this triangle data into the edgelist
			edgelist[x] = new medge();
			edgelist[x].index1 = trianglelist[i].index1;
			edgelist[x].index2 = trianglelist[i].index2;
			edgelist[x].pos1 = trianglelist[i].pos1;
			edgelist[x].pos2 = trianglelist[i].pos2;
			edgelist[x+1] = new medge();
			edgelist[x+1].index1 = trianglelist[i].index2;
			edgelist[x+1].index2 = trianglelist[i].index3;
			edgelist[x+1].pos1 = trianglelist[i].pos2;
			edgelist[x+1].pos2 = trianglelist[i].pos3;
			edgelist[x+2] = new medge();
			edgelist[x+2].index1 = trianglelist[i].index3;
			edgelist[x+2].index2 = trianglelist[i].index1;
			edgelist[x+2].pos1 = trianglelist[i].pos3;
			edgelist[x+2].pos2 = trianglelist[i].pos1;
			//half geometry
			hface[i] = new half.face();
			//triangle and edge
			hface[i].triangle = trianglelist[i];
			hface[i].medge = edgelist[x];
			//edge
			hface[i].edge = new half.edge();
			hface[i].edge.currentmedge = edgelist[x];
			hface[i].edge.endvert = new half.vert();
			hface[i].edge.endvert.edge = hface[i].edge;
			hface[i].edge.endvert.position = edgelist[x].pos2;
			hface[i].edge.endvert.medge = edgelist[x];
			hface[i].edge.nextmedge = edgelist[x+1];
			hface[i].edge.face = hface[i];
			//next edge
			hface[i].edge.nexthalfedge = new half.edge();
			hface[i].edge.nexthalfedge.currentmedge = edgelist[x+1];
			hface[i].edge.nexthalfedge.endvert = new half.vert();
			hface[i].edge.nexthalfedge.endvert.edge = hface[i].edge.nexthalfedge;
			hface[i].edge.nexthalfedge.endvert.position = edgelist[x+1].pos2;
			hface[i].edge.nexthalfedge.endvert.medge = edgelist[x+1];
			hface[i].edge.nexthalfedge.nextmedge = edgelist[x+2];
			hface[i].edge.nexthalfedge.face = hface[i];
			//inception
			hface[i].edge.nexthalfedge.nexthalfedge = new half.edge();
			hface[i].edge.nexthalfedge.nexthalfedge.currentmedge = edgelist[x+2];
			hface[i].edge.nexthalfedge.nexthalfedge.endvert = new half.vert();
			hface[i].edge.nexthalfedge.nexthalfedge.endvert.edge = hface[i].edge.nexthalfedge.nexthalfedge;
			hface[i].edge.nexthalfedge.nexthalfedge.endvert.position = edgelist[x+2].pos2;
			hface[i].edge.nexthalfedge.nexthalfedge.endvert.medge = edgelist[x+2];
			hface[i].edge.nexthalfedge.nexthalfedge.nextmedge = edgelist[x];
			hface[i].edge.nexthalfedge.nexthalfedge.face = hface[i];
			//hedge
			hedge[x] = hface[i].edge;
			hedge[x+1] = hface[i].edge.nexthalfedge;
			hedge[x+2] = hface[i].edge.nexthalfedge.nexthalfedge;
			//hvert
			hvert[x] = new half.vert();
			hvert[x].edge = hface[i].edge;
			hvert[x].position = edgelist[x].pos2;
			hvert[x].medge = edgelist[x];
			hvert[x+1] = new half.vert();
			hvert[x+1].edge = hface[i].edge.nexthalfedge;
			hvert[x+1].position = edgelist[x+1].pos2;
			hvert[x+1].medge = edgelist[x+1];
			hvert[x+2] = new half.vert();
			hvert[x+2].edge = hface[i].edge.nexthalfedge.nexthalfedge;
			hvert[x+2].position = edgelist[x+2].pos2;
			hvert[x+2].medge = edgelist[x+2];
			i++;}}
	
	void CreateHalfGeometry(){
		int i=0, j=0;
		//enumerate edges relative to each other to find twin edges
		j=0; while (j< hedge.Length){
			i=j+1;while (i< hedge.Length){
				if (//if different index but same inverse position (clone)
						hedge[j].currentmedge.index1	!= hedge[i].currentmedge.index1
					&&	hedge[j].currentmedge.index2	!= hedge[i].currentmedge.index2
					&&	hedge[j].currentmedge.pos1		== hedge[i].currentmedge.pos2
					&&	hedge[j].currentmedge.pos2		== hedge[i].currentmedge.pos1
					||//if same inverse index (twin)
					hedge[j].currentmedge.index1		== hedge[i].currentmedge.index2
					&&	hedge[j].currentmedge.index2	== hedge[i].currentmedge.index1)
				{//set both are twinedge to eachother
					hedge[i].twinedge	= hedge[j];
					hedge[i].twinmedge	= hedge[j].currentmedge;					
					hedge[j].twinedge	= hedge[i];					
					hedge[j].twinmedge	= hedge[i].currentmedge;}
				i++;}
			j++;}}
	
	void EnumerateEdge(){
		//visualization list to verify structures
		int similar = 0;
		int clone = 0;
		int twin = 0;
		int j,i=0;
		twin[] twintemp		= new twin[edgelist.Length];
		twin[] clonetemp	= new twin[edgelist.Length];
		i=0; while (i < twintemp.Length){
			twintemp[i]		= new twin();
			i++;}
		i=0; while (i < clonetemp.Length){
			clonetemp[i]	= new twin();
			i++;}
		j=0; while (j < edgelist.Length){
			i=j+1;while (i< edgelist.Length){
				//find edge that are similar, then are reverse		
				//similar
				if (edgelist[j].index1 == edgelist[i].index1
					&& edgelist[j].index2 == edgelist[i].index2){
					similar++;}
				//-----check also position to find separation (same pos !index)
				if (edgelist[j].index1 != edgelist[i].index1
					&& edgelist[j].index2 != edgelist[i].index2
					&& edgelist[j].pos1 == edgelist[i].pos1
					&& edgelist[j].pos2 == edgelist[i].pos2
					||
					edgelist[j].index1 != edgelist[i].index1
					&& edgelist[j].index2 != edgelist[i].index2
					&& edgelist[j].pos1 == edgelist[i].pos2
					&& edgelist[j].pos2 == edgelist[i].pos1)
				{
					clonetemp[clone].n1 = edgelist[j];
					clonetemp[clone].n2 = edgelist[i];
					clone++;}
				//twin
				if (edgelist[j].index1 == edgelist[i].index2
					&& edgelist[j].index2 == edgelist[i].index1){
					twintemp[twin].n1 = edgelist[j];
					twintemp[twin].n2 = edgelist[i];
					twin++;}
				i++;}
			j++;}
		//count twin
		twinlist = new twin[twin];
		i = 0; while(i < twin){
			twinlist[i] = twintemp[i];
			i++;}
		//count solo edge against the twinlist
		int s = 0;
		medge[] solo = edgelist;
		i = 0; while(i < solo.Length){
			j = 0; while (j < twinlist.Length){
				if (twinlist[j].n1 == solo[i]){
					solo[i] = null;s++;}
				if (twinlist[j].n2 == solo[i]){
					solo[i] = null;s++;}
				j++;}
			i++;}
		sololist = new medge[s];
		j=0;
		i = 0; while(i < solo.Length){
			if (solo[i] != null){
				sololist[j] = solo[i];
				j++;}
			i++;}
		//edge with different index but nonetheless twin with same pos,
		//aka separated by seam
		clonelist = new twin[clone];
		i = 0; while(i < clone){
			clonelist[i] = clonetemp[i];
			i++;}
		s = 0;
		medge[] single = edgelist;
		i = 0; while(i < solo.Length){
			j = 0; while (j < clonelist.Length){
				if (clonelist[j].n1 == single[i]){
					s++;}
				if (clonelist[j].n2 == single[i]){
					s++;}
				j++;}
			i++;}
		singlelist = new medge[s];
		j=0;
		i = 0; while(i < single.Length){
			if (single[i] != null){
				singlelist[j] = single[i];
				j++;}
			i++;}}
	
	//*********************************************************
	public void setup (){
		//UnityMesh = this.gameObject.GetComponent<MeshFilter>().mesh; only for monobehavior
		if (UnityMesh != null){
			CreateTriangleFromMesh();
			CreateMeshEdge();
			CreateHalfGeometry();
			EnumerateEdge();}
		else{Debug.LogError("mesh is null");}}
	
	public void DrawDebugHedge(){		
		foreach (half.edge e in hedge){
			if (e != null ) Debug.DrawLine(e.twinmedge.pos1,e.twinmedge.pos2,Color.green);}}
	//drawsolo
	//drawtwin
	//drawclone
}