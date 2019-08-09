import React from 'react';
import ContentArea from './components/contentArea';
import XhtmlString from './components/xhtmlString';
export type EpiPropertyType = 'ContentArea' | 'BlockData' | 'XhtmlString';

export interface IEpiProperty {
  $component?: string;
  $propertyType?: EpiPropertyType;
}

interface IProps {
  for: IEpiProperty; // Todo: Remove any if the typescript generators is done
}

export default (props: IProps) => {
  if (!props.for) {
    return null;
  }
  switch (props.for.$propertyType) {
    case 'ContentArea':
      return <ContentArea {...(props.for as any)} />;
    case 'XhtmlString':
      return <XhtmlString {...(props.for as any)} />;
  }
  return <div>In prop</div>;
};
