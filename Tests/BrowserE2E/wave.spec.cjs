const {test,expect}=require('@playwright/test');
test('one Start click launches real WASM enemies and pause stops them',async({page})=>{
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto('/');await expect(page.locator('#menu')).toBeVisible({timeout:60000});
 await page.locator('#continue').click();await page.getByRole('button',{name:'Đánh thức người bảo vệ',exact:true}).click();
 await page.locator('#start-wave').click();
 await expect.poll(()=>page.evaluate(()=>window.guardian.getState()?.enemies.length),{timeout:15000}).toBeGreaterThan(0);
 await expect.poll(()=>page.evaluate(()=>window.guardian.getState()?.enemies[0]?.position.x),{timeout:15000}).toBeGreaterThan(0);
 const state=await page.evaluate(()=>window.guardian.getState());expect(state.tutorial).toBe(4);expect(state.towers[0].level).toBe(2);expect(state.wave).toBe(1);
 await page.locator('#pause').click();const before=await page.evaluate(()=>window.guardian.getState().elapsed);await page.waitForTimeout(1200);expect(await page.evaluate(()=>window.guardian.getState().elapsed)).toBe(before);
 await page.getByRole('button',{name:'Tiếp tục',exact:true}).click();await expect.poll(()=>page.evaluate(()=>window.guardian.getState().elapsed)).toBeGreaterThan(before);
 await expect.poll(()=>page.evaluate(()=>window.guardian.getVfxStats().emitted),{timeout:20000}).toBeGreaterThan(0);
 await expect.poll(()=>page.evaluate(()=>window.guardian.getVfxStats().drawn),{timeout:10000,intervals:[50]}).toBeGreaterThan(0);
 await page.screenshot({path:'test-results/wave-running.png',fullPage:true});expect(errors).toEqual([]);
});
