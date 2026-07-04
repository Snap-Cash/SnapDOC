(function(){
  const root=document.documentElement;
  const savedTheme=localStorage.getItem('snap.theme')||'light';
  const savedColor=localStorage.getItem('snap.primary');
  root.setAttribute('data-theme',savedTheme);
  if(savedColor) root.style.setProperty('--snap-primary',savedColor);
  window.SnapUI={
    toggleTheme(){const next=root.getAttribute('data-theme')==='dark'?'light':'dark';root.setAttribute('data-theme',next);localStorage.setItem('snap.theme',next);this.toast(next==='dark'?'تم تفعيل الوضع الداكن':'تم تفعيل الوضع الفاتح')},
    setPrimary(color){root.style.setProperty('--snap-primary',color);localStorage.setItem('snap.primary',color);this.toast('تم تغيير لون الهوية')},
    toast(msg){let host=document.querySelector('.snap-toast-host');if(!host){host=document.createElement('div');host.className='snap-toast-host';document.body.appendChild(host)}const t=document.createElement('div');t.className='snap-toast';t.textContent=msg;host.appendChild(t);setTimeout(()=>t.remove(),2800)},
    openCommand(){document.querySelector('.snap-command')?.classList.add('open');setTimeout(()=>document.querySelector('.snap-command input')?.focus(),10)},
    closeCommand(){document.querySelector('.snap-command')?.classList.remove('open')},
    toggleSidebar(){(document.getElementById('snapSidebar')||document.querySelector('.snap-sidebar')||document.querySelector('.snap-sidebar-v2'))?.classList.toggle('open')},
    openDrawer(id){document.getElementById(id)?.classList.add('open')},
    closeDrawer(id){document.getElementById(id)?.classList.remove('open')}
  };
  document.addEventListener('keydown',e=>{if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='k'){e.preventDefault();SnapUI.openCommand()} if(e.key==='Escape') SnapUI.closeCommand();});
  document.addEventListener('click',e=>{if(e.target.closest('[data-theme-toggle]'))SnapUI.toggleTheme();if(e.target.closest('[data-sidebar-toggle]'))SnapUI.toggleSidebar();if(e.target.matches('.snap-command'))SnapUI.closeCommand();});
})();
