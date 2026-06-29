import{c,a1 as d,v as s,M as n}from"./index-BRIKTQ8y.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const u=c("clock",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"M12 6v6l4 2",key:"mmk7yg"}]]),l=d({id:"dashboard",state:()=>({dashboards:[],loading:!1}),actions:{async loadDashboards(a){try{const e=s(),r=n(),t=r.currentProjectId?{projectId:r.currentProjectId}:{},o=await e.executeCommand("LoadDashboards",t,a);o&&o.Data&&(this.dashboards=o.Data)}catch(e){console.error("Error loading dashboards:",e),this.dashboards=[]}},async saveDashboard(a,e){try{return await s().executeCommand("SaveDashboard",{dashboard:a},e)}catch(r){throw console.error("Error saving dashboard:",r),r}},async deleteDashboard(a,e){try{return await s().executeCommand("DeleteDashboard",{id:a},e),!0}catch(r){throw console.error("Error deleting dashboard:",r),r}}}});export{u as C,l as u};
