const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(require('node:path').join(__dirname,'../BrowserDemo/wwwroot/game.js'),'utf8');
const handler=source.match(/function startOrAdvanceWave\(\)\{[\s\S]*?\n\}/)[0];
let calls=[];const context={state:{tutorial:2,towers:[{kind:0,pad:4}]},selected:-1,choice:0,skill:-1,renderHud(){},sound(){},command(action,index){calls.push([action,index]);if(action==='upgrade')context.state.tutorial=4;if(action==='wave')context.state.tutorial=2;}};
vm.runInNewContext(handler+';startOrAdvanceWave();',context);
assert.deepEqual(calls,[['upgrade',0]]);assert.equal(context.state.tutorial,4);assert.equal(context.selected,4);assert.equal(context.choice,-1);
calls=[];context.state.tutorial=1;vm.runInNewContext('startOrAdvanceWave()',context);assert.equal(calls[0][0],'wave');assert.equal(context.selected,4);
assert(source.includes("hidden=state.phase!==0&&state.tutorial!==2"));
console.log('PASS: tutorial continuation upgrades the required tower, clears placement, never starts a second wave, and remains visible while combat is held.');
