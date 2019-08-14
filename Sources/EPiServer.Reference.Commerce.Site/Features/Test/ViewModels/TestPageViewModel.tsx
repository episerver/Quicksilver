import React from 'react';
import EpiProperty from 'features/EpiProperty';

type PropType = {
  title: string;
  currentPage: {
    contentLink: string;
    myBlocks:any;
  };
};

function TEST(props: PropType) {
  return (
    <div>
      TEST - {props.title} {props.currentPage.contentLink}
      <EpiProperty for={props.currentPage.myBlocks} />
    </div>
  );
}

export default TEST;
