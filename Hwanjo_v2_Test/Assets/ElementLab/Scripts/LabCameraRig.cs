using System.Collections.Generic;
using UnityEngine;

namespace Hwanjo.ElementLab
{
    public sealed class LabCameraRig
    {
        public bool Damping=true;
        public Vector2 Velocity {get;private set;}
        public Vector3 Position {get;private set;}
        readonly LabWorld world;
        readonly List<Transform> layers=new List<Transform>();
        readonly float[] factors={.15f,.4f,.7f};
        readonly Vector3 referenceCamera=new Vector3(5,3.4f,-10);
        public LabCameraRig(LabWorld world)
        {
            this.world=world;
            if(world.Art && world.Art.Far && world.Art.Middle)
            {
                MakeLayer("遠 · 산과 누각",world.Art.Far,0,-50);
                MakeLayer("中 · 숲과 석문",world.Art.Middle,1,-35);
                MakeLayer("近 · 비충돌 가지",world.Art.Terrain[7],2,-12);
            }
            Snap();
        }
        void MakeLayer(string name,Sprite sprite,int index,int order)
        {
            var root=new GameObject(name).transform;root.SetParent(world.transform);layers.Add(root);
            for(int i=-4;i<=4;i++)
            {
                var go=new GameObject("Layer segment "+i);go.transform.SetParent(root,false);
                var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=order;
                if(index<2)
                {
                    float height=index==0?15:13;float width=height*sprite.bounds.size.x/sprite.bounds.size.y;
                    go.transform.localScale=Vector3.one*height/sprite.bounds.size.y;
                    go.transform.localPosition=new Vector3(i*width,4.4f,0);sr.flipX=i%2!=0;
                    sr.color=index==0?new Color(.64f,.76f,.76f):new Color(.78f,.86f,.8f);
                }
                else
                {
                    go.transform.localScale=Vector3.one*2.0f/sprite.bounds.size.y;
                    go.transform.localPosition=new Vector3(i*9,-1.1f,0);sr.flipX=i%2!=0;
                    sr.color=new Color(.52f,.65f,.6f);
                }
            }
        }
        public void Snap() {Position=world.CameraGoal();Velocity=Vector2.zero;world.Camera.transform.position=Position;Parallax();}
        public Vector3 Tick(float dt)
        {
            Vector3 goal=world.CameraGoal();Vector2 v=Velocity;
            if(!Damping) {Position=goal;v=Vector2.zero;}
            else if(dt>0)Position=new Vector3(Mathf.SmoothDamp(Position.x,goal.x,ref v.x,world.Tuning.CameraDampingX,Mathf.Infinity,dt),Mathf.SmoothDamp(Position.y,goal.y,ref v.y,world.Tuning.CameraDampingY,Mathf.Infinity,dt),-10);
            Velocity=v;return Position;
        }
        public void Parallax()
        {
            for(int i=0;i<layers.Count;i++)
            {
                // factor = screen-relative travel / actual camera travel. World geometry is never scaled.
                // Absolute reference formula prevents drift during backtracking and teleporting.
                Vector3 delta=world.Camera.transform.position-referenceCamera;delta.z=0;
                layers[i].position=new Vector3(5,0,0)+delta*(1-factors[i]);
            }
        }
    }
}
