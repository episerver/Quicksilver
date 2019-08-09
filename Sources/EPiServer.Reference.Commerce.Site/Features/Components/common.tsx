import React from 'react';

export const ItemCloneUpdate = (
  item: any,
  configureProps?: (element: React.ReactElement<any>) => any
) => {
  const generatedItem = item as React.ReactElement<any>;
  const element = React.Children.only(generatedItem);
  return React.cloneElement(element, configureProps(element));
};
