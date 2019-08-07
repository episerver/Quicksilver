import React from 'react';

const AvailableTypes: { [key: string]: React.LazyExoticComponent<any> } = {
  'EPiServer.Reference.Commerce.Site.Features.Start.ViewModels.StartPageViewModel': React.lazy(
    () =>
      import(
        /* webpackChunkName: "StartPageViewModel" */ 'features/Start/ViewModels/StartPageViewModel'
      )
  ),
  'EPiServer.Reference.Commerce.Site.Features.Search.ViewModels.SearchViewModel': React.lazy(
    () =>
      import(
        /* webpackChunkName: "SearchViewModel" */ 'features/Search/ViewModels/SearchViewModel'
      )
  )
};

export default AvailableTypes;
