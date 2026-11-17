'use strict';
// Presentation only: listens to actual tower shot counters, never applies damage.
window.GuardianCombatVfx=class {
 constructor(){this.reset()}
 reset(){this.shots=new Map();this.bursts=[];this.time=0;this.emitted=0;this.drawn=0}
 sync(state,level){this.time=state.elapsed;const active=new Set(state.towers.map(t=>t.pad));for(const key of this.shots.keys())if(!active.has(key))this.shots.delete(key);
 for(const t of state.towers){const previous=this.shots.get(t.pad)||0;this.shots.set(t.pad,t.shots);if(t.shots<=previous||t.kind===2)continue;const from=level.pads[t.pad].position;const fx=[...state.effects].reverse().find(f=>(f.kind===t.kind||t.kind===3&&f.kind===14)&&Math.hypot(f.from.x-from.x,f.from.y-from.y)<.1);if(!fx)continue;
 this.bursts.push({pad:t.pad,kind:t.kind,from:{x:(from.x+.5)*60,y:(from.y+.5)*60-30},to:{x:(fx.to.x+.5)*60,y:(fx.to.y+.5)*60},born:this.time,seed:t.shots*17+t.pad});this.emitted++;
 }this.bursts=this.bursts.filter(b=>this.time-b.born<.7).slice(-100)}
 recoil(pad){const b=this.bursts.findLast(b=>b.pad===pad&&this.time-b.born<.14);return b?Math.sin((this.time-b.born)/.14*Math.PI)*5:0}
 draw(ctx){this.drawn=0;for(const b of this.bursts){const age=this.time-b.born;if(age<0||age>.7)continue;this.drawn++;ctx.save();const color=b.kind===1?'#8de8ff':b.kind===4?'#a9f36b':b.kind===3?'#ffb46c':'#e8ef9e';ctx.globalCompositeOperation='lighter';ctx.strokeStyle=color;ctx.fillStyle=color;ctx.shadowColor=color;
 if(b.kind===3){const t=age/.7,alpha=1-t;ctx.globalAlpha=alpha;ctx.shadowBlur=16;ctx.lineWidth=5*(1-t)+1;ctx.beginPath();ctx.arc(b.from.x,b.from.y+30,15+t*100,0,Math.PI*2);ctx.stroke();this.sparks(ctx,{x:b.from.x,y:b.from.y+30},age,20,color,b.seed,120);ctx.restore();continue}
 const flight=.28,t=Math.min(1,age/flight),dx=b.to.x-b.from.x,dy=b.to.y-b.from.y,len=Math.hypot(dx,dy)||1,x=b.from.x+dx*t,y=b.from.y+dy*t-(b.kind===4?Math.sin(t*Math.PI)*22:0);
 if(age<.12){ctx.globalAlpha=1-age/.12;ctx.shadowBlur=22;ctx.beginPath();ctx.arc(b.from.x,b.from.y,6+age*45,0,Math.PI*2);ctx.fill();ctx.globalAlpha=1}
 if(age<=flight){ctx.shadowBlur=12;ctx.lineWidth=b.kind===1?4:3;ctx.beginPath();ctx.moveTo(x-dx/len*24,y-dy/len*24);ctx.lineTo(x,y);ctx.stroke();ctx.save();ctx.translate(x,y);ctx.rotate(Math.atan2(dy,dx));
 if(b.kind===4){ctx.beginPath();ctx.ellipse(0,0,8,5,0,0,Math.PI*2);ctx.fill()}else{ctx.beginPath();ctx.moveTo(10,0);ctx.lineTo(-7,-5);ctx.lineTo(-3,0);ctx.lineTo(-7,5);ctx.closePath();ctx.fill()}ctx.restore();}
 else{const hit=age-flight;ctx.globalAlpha=Math.max(0,1-hit/.42);ctx.shadowBlur=12;ctx.lineWidth=2;ctx.beginPath();ctx.arc(b.to.x,b.to.y,5+hit*(b.kind===1?75:45),0,Math.PI*2);ctx.stroke();this.sparks(ctx,b.to,hit,b.kind===1?12:8,color,b.seed,55);if(b.kind===1){ctx.translate(b.to.x,b.to.y);for(let i=0;i<6;i++){ctx.rotate(Math.PI/3);ctx.beginPath();ctx.moveTo(0,0);ctx.lineTo(0,15+hit*25);ctx.stroke()}}}
 ctx.restore();}}
 sparks(ctx,p,t,n,color,seed,speed){ctx.fillStyle=color;for(let i=0;i<n;i++){const a=i/n*Math.PI*2+seed*.7,d=t*speed*(.5+(i%4)*.2);ctx.beginPath();ctx.arc(p.x+Math.cos(a)*d,p.y+Math.sin(a)*d+t*t*45,Math.max(.5,3-t*4),0,Math.PI*2);ctx.fill()}}
 stats(){return {emitted:this.emitted,active:this.bursts.length,drawn:this.drawn,time:this.time}}
};
