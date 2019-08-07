import AvailableTypes from 'features/Components/availableTypes';
import React, { Suspense } from 'react';
import ReactDOM from 'react-dom';

const CurrentComponent = AvailableTypes[(window as any).CURRENT_TYPE];

ReactDOM.render(
  <Suspense fallback={<div>Loading...</div>}><CurrentComponent {...(window as any).CURRENT_PAGE} /></Suspense>,
  document.getElementById('main-content')
);

if ((module as any).hot) {
  (module as any).hot.accept();
}
