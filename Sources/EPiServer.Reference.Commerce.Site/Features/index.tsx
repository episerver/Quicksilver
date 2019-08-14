import AvailableTypes from 'features/Components/availableTypes';
import MainNavigation from 'features/Navigation/ViewModels/Navigation';
import React, { Suspense } from 'react';
import ReactDOM from 'react-dom';

const CurrentComponent =
  AvailableTypes[(window as any).CURRENT_PAGE['$component']];

ReactDOM.render(
  <div>
    <MainNavigation />
    <Suspense fallback={<div>Loading...</div>}>
      <CurrentComponent {...(window as any).CURRENT_PAGE} />
    </Suspense>
  </div>,
  document.getElementById('main-content')
);

if ((module as any).hot) {
  (module as any).hot.accept();
}
