using System.Collections;
using System.Collections.Generic;// ise for using list
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)] public float viewAngle;
    //[SerializeField]Renderer targetRenderer;
    
    

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();
    public float meshResolution;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;
    
    public int edgeResolveIterations=10;
    
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    private void Start() {
        
        viewMesh=new Mesh();
        viewMesh.name="View Mesh";
        viewMeshFilter.mesh=viewMesh;

        StartCoroutine("FindTargetWithDelay",0.2f);
    }
   
   IEnumerator FindTargetWithDelay(float delay){
       while(true){
           yield return new WaitForSeconds(delay);
           FindVisibleTarget();
       }
   }
   
   private void LateUpdate() {
       DrawFieldOfView();   //Draw field of view must be called after the frame...
   }

    void FindVisibleTarget(){
        
        visibleTargets.Clear();// clear all the list...
        
        Collider[] targetInViewRadius=Physics.OverlapSphere(transform.position,viewRadius+2,targetMask);

        for(int i=0;i<targetInViewRadius.Length;i++){
            Transform target=targetInViewRadius[i].transform;
            Vector3 dirToTarget= (target.position-transform.position).normalized;

            if(Vector3.Angle(transform.forward,dirToTarget)< viewAngle/2){
                float dstToTarget=Vector3.Distance(transform.position,target.position);
                
                if(!Physics.Raycast(transform.position,dirToTarget,dstToTarget,obstacleMask)){
                    // not obstacle present here..
                    visibleTargets.Add(target);

                    target.gameObject.GetComponent<Renderer>().material.color=Color.red;
                   
                }
               
            }else if(Vector3.Angle(transform.forward,dirToTarget)> viewAngle/2){
                    Debug.Log("Reaching");
                    target.gameObject.GetComponent<Renderer>().material.color=Color.blue;
                }
            
            if(Vector3.Distance(target.position,transform.position)>=viewRadius){
                target.gameObject.GetComponent<Renderer>().material.color=Color.blue;

            }
         }
    }
        
    
    void DrawFieldOfView(){

        int stepCount= Mathf.RoundToInt(viewAngle*meshResolution);
        float stepAngleSize =viewAngle/stepCount;


        List<Vector3> viewPoints=new List<Vector3>(); //We will maintain a list of all Vector3 for all the point that our raycast hit...

        ViewCastInfo oldViewCast=new ViewCastInfo() ;

        for(int i=0;i<=stepCount;i++){
           
            float angle=transform.eulerAngles.y-viewAngle/2 +stepAngleSize*i;  /// angle is a golbal angle by reltibe wrt transform.
             
           // Debug.DrawLine(transform.position,transform.position+ DirFromAngle(angle,true)*viewRadius,Color.red);

            ViewCastInfo newViewCast= ViewCast (angle);

            if(i>0){
                if(oldViewCast.hit!=newViewCast.hit){
                    EdgeInfo edge=FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA!=Vector3.zero){
                        viewPoints.Add(edge.pointA);
                    }
                    if(edge.pointB!=Vector3.zero){
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            viewPoints.Add(newViewCast.point);  
            oldViewCast=newViewCast;    // to maintain all the point of viewCast hif... so that we can constract the mesh...
        }


        int vertexCount= viewPoints.Count+1; //  +1 for origin i.e. transform  point vertex
        Vector3[] vertices =new Vector3[vertexCount];
        int [] triangles=new int [(vertexCount-2)*3];

        vertices[0] = Vector3.zero;

        for (int i=0;i<vertexCount-1;i++){
           // vertices[i+1]=viewPoints[i]; THis is in local space we want veritice wrt character i.e. use transform.InverseTransformPoint..

            vertices[i+1]=transform.InverseTransformPoint( viewPoints[i]);
            //Debug.DrawLine(transform.position,viewPoints[i],Color.red);
            if(i<vertexCount-2){
                triangles[i*3]=0;
                triangles[i*3+1]= i+1;
                triangles[i*3+2]=i+2;
            }
            

        }
        viewMesh.Clear();
        viewMesh.vertices=vertices;
        viewMesh.triangles=triangles;
        viewMesh.RecalculateNormals();

    } 
       EdgeInfo FindEdge(ViewCastInfo minViewCast,ViewCastInfo maxViewCast){
            float minAngle=minViewCast.angle;
            float maxAngle=maxViewCast.angle;

            Vector3 minPoint=Vector3.zero;
            Vector3 maxPoint=Vector3.zero;

            for(int i=0;i<edgeResolveIterations;i++){
                float angle=(minAngle+maxAngle)/2;
                ViewCastInfo newViewCast=ViewCast(angle);
                
                if(newViewCast.hit==minViewCast.hit){
                    minAngle= angle;
                    minPoint= newViewCast.point;
                }else{
                    maxAngle=angle;
                    maxPoint=newViewCast.point;
                }
            }
            return new EdgeInfo(minPoint,maxPoint);
        }
    ViewCastInfo ViewCast(float globalAngle){

        Vector3 dir=DirFromAngle(globalAngle,true);
        RaycastHit hit;
        if(Physics.Raycast(transform.position,dir,out hit,viewRadius,obstacleMask)){

            return new ViewCastInfo(true,hit.point,hit.distance,globalAngle);
            
        }else{
            return new ViewCastInfo(false,transform.position+dir*viewRadius,viewRadius,globalAngle);
            //return new ViewCastInfo(false,dir*viewRadius,viewRadius,globalAngle);
        }

    }
    
    public Vector3 DirFromAngle(float angleInDegree,bool angleIsGlobal){
        
        if(!angleIsGlobal){
           
            // convert into global by adding transform.angle
            angleInDegree+=transform.eulerAngles.y;
        }
        // we return the co-ordinate of that point,but without multiplying the radius of circle
        return new Vector3 (Mathf.Sin(angleInDegree*Mathf.Deg2Rad),0,Mathf.Cos(angleInDegree*Mathf.Deg2Rad));
        

        //return new Vector3 (Mathf.Cos(angleInDegree*Mathf.Rad2Deg),0,Mathf.Sin(angleInDegree*Mathf.Rad2Deg));
    }
        
    public struct ViewCastInfo{
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit,Vector3 _point,float _dst,float _angle){
            hit=_hit;
            point=_point;
            dst=_dst;
            angle=_angle; 
        }
    }
     public struct EdgeInfo{
            public Vector3 pointA;
            public Vector3 pointB;

            public EdgeInfo(Vector3 _pointA,Vector3 _pointB){
                pointA=_pointA;
                pointB=_pointB;   
            }
        }
}
