using System;
using System.Collections.Generic;
namespace EcoGuardian.Core
{
    public static class Campaign
    {
        static Group G(EnemyKind k,int n){return new Group(k,n);}
        static Wave W(params Group[] g){return new Wave(g);}
        static Route R(params float[] v){var r=new Route();for(int i=0;i<v.Length;i+=2)r.points.Add(new V(v[i],v[i+1]));return r;}
        public static Level Create(int stage)
        {
            if(stage<1||stage>5)throw new ArgumentOutOfRangeException(nameof(stage));
            var l=new Level{index=stage};
            string[] titles={"Tiếng động trong rừng","Dòng suối bị đầu độc","Đêm lửa rừng","Cuộc tấn công tổng lực","Trái tim Đại Ngàn"};
            string[] subs={"Đánh thức những người bảo vệ đầu tiên.","Giữ hai tuyến đường. Hồi sinh dòng suối.","Dùng Mưa Rừng để cứu những ô đang cháy.","Phá vùng nhiễu. Giữ vững ba cổng.","Cây Mẹ đang gọi. Hãy chặn Titan Lumber-X."};
            string[] ends={"Đây chỉ là đội trinh sát. Hãy theo dòng suối, ta cảm nhận được điều bất thường.","Chúng không chỉ lấy đi cây. Chúng đang lấy cả nguồn sống.","Ta vẫn còn thời gian, nhưng đoàn máy chính đã tới.","Cỗ máy ấy đang hướng thẳng về Cây Mẹ.","Đại Ngàn đã sống qua đêm nay. Ngày mai, chúng ta sẽ trồng lại những gì đã mất."};
            l.title=titles[stage-1];l.subtitle=subs[stage-1];l.ending=ends[stage-1];
            l.routes.Add(R(-1,2,4,2,4,7,10,7,10,4,16,4,16,5,19,5));
            if(stage>=2)l.routes.Add(stage==3?R(-1,9,7,9,7,4,10,4,16,4,16,5,19,5):R(-1,9,7,9,7,7,10,7,10,4,16,4,16,5,19,5));
            if(stage>=4)l.routes.Add(R(8,-1,8,2,4,2,4,7,10,7,10,4,16,4,16,5,19,5));
            // Select spaced, adjacent build pads; two traps per map, never on a route.
            var candidates=new List<V>{new V(3,3),new V(5,6),new V(8,6),new V(11,5),new V(13,3),new V(15,5),new V(3,6),new V(9,4),new V(6,8),new V(9,8),new V(1,8),new V(6,3),new V(7,1),new V(11,3),new V(15,3),new V(17,4),new V(2,3),new V(5,3),new V(12,5),new V(3,8),new V(8,8)};
            int count=8+(stage-1)*2;
            foreach(var p in candidates){if(OnPath(l,p)||l.pads.Count>=count)continue;l.pads.Add(new Pad(p,l.pads.Count==1||l.pads.Count==5));}
            // Polluted sites are additional blocked sites, leaving ten usable pads in level 2.
            if(stage==2){l.pads.Add(new Pad(new V(1,6),false,true));l.pads.Add(new Pad(new V(2,6),false,true));l.pads.Add(new Pad(new V(2,7),false,true));}
            switch(stage){
            case 1:l.waves.AddRange(new[]{W(G(EnemyKind.Scout,5)),W(G(EnemyKind.Scout,8)),W(G(EnemyKind.Scout,5),G(EnemyKind.Lumber,3)),W(G(EnemyKind.Scout,10),G(EnemyKind.Lumber,5)),W(G(EnemyKind.Heavy,1),G(EnemyKind.Scout,6))});break;
            case 2:l.waves.AddRange(new[]{W(G(EnemyKind.Scout,8),G(EnemyKind.Lumber,4)),W(G(EnemyKind.Scout,8),G(EnemyKind.Speed,4)),W(G(EnemyKind.Lumber,10),G(EnemyKind.Speed,6)),W(G(EnemyKind.Bulldozer,2),G(EnemyKind.Scout,8)),W(G(EnemyKind.Toxic,1),G(EnemyKind.Lumber,8),G(EnemyKind.Speed,6))});break;
            case 3:l.waves.AddRange(new[]{W(G(EnemyKind.Lumber,10),G(EnemyKind.Scout,6)),W(G(EnemyKind.Speed,10),G(EnemyKind.Lumber,6)),W(G(EnemyKind.Bulldozer,3),G(EnemyKind.Scout,8)),W(G(EnemyKind.Bulldozer,2),G(EnemyKind.Lumber,10),G(EnemyKind.Speed,8)),W(G(EnemyKind.Fire,1),G(EnemyKind.Lumber,10),G(EnemyKind.Speed,8))});break;
            case 4:l.waves.AddRange(new[]{W(G(EnemyKind.Scout,12),G(EnemyKind.Lumber,8)),W(G(EnemyKind.Speed,12),G(EnemyKind.Jammer,2)),W(G(EnemyKind.Bulldozer,4),G(EnemyKind.Lumber,12)),W(G(EnemyKind.Jammer,3),G(EnemyKind.Speed,12),G(EnemyKind.Lumber,10)),W(G(EnemyKind.Bulldozer,2),G(EnemyKind.Speed,10),G(EnemyKind.Lumber,15))});break;
            case 5:l.waves.AddRange(new[]{W(G(EnemyKind.Scout,12),G(EnemyKind.Lumber,10)),W(G(EnemyKind.Speed,10),G(EnemyKind.Bulldozer,3)),W(G(EnemyKind.Jammer,3),G(EnemyKind.Lumber,12)),W(G(EnemyKind.Bulldozer,4),G(EnemyKind.Speed,12),G(EnemyKind.Jammer,2)),W(G(EnemyKind.Titan,1))});break;
            }
            return l;
        }
        public static bool OnPath(Level l,V p){foreach(var r in l.routes)for(int i=1;i<r.points.Count;i++)if(SegmentDistance(p,r.points[i-1],r.points[i])<.65f)return true;return false;}
        public static float SegmentDistance(V p,V a,V b){float dx=b.x-a.x,dy=b.y-a.y;float n=dx*dx+dy*dy;float t=n==0?0:Math.Max(0,Math.Min(1,((p.x-a.x)*dx+(p.y-a.y)*dy)/n));return V.Distance(p,V.Lerp(a,b,t));}
    }
}
