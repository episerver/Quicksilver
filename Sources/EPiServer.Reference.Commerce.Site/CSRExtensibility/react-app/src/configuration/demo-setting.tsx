import * as React from 'react';
import { AppSettingState, HttpRequest } from '@episerver/csr-extension';
import { OrderDetailTab } from '../demo/class-component';
import { ReactHookComponent } from '../demo/react-hook-component';
import { ReduxContainerComponent } from '../demo/redux-container-component';
import store from '../store';
import * as ApplicationActions from '../store/actions';
import { Provider } from 'react-redux';

const PlaceHolder: React.FunctionComponent = (props) => {
  return <div style={{padding: '15px', border: '1px blue solid'}}>
    {props.children}
  </div>
}

export const setting = {
  pageConfig: {
    /**
     * Config page at route '/orders'
     */
    orderList: {
      extendUI: {
        /**
         * Put component at the top of page
         */
        topPlaceHolder: () => <PlaceHolder>Orders Top Placeholder</PlaceHolder>,

        /**
         * Put component at the bottom of page
         */
        bottomPlaceHolder: () => <PlaceHolder>Orders Bottom Placeholder</PlaceHolder>,
      },
    },

    /**
     * Config page at route '/orders/:id'
     */
    orderDetail: {
      extendUI: {
        topPlaceHolder: (props) => {
          // Return function component. 
          return <PlaceHolder><ReactHookComponent /></PlaceHolder>
        },
        bottomPlaceHolder: (props) => {
          HttpRequest.get('csr-demo/getData').then(resp => {
            store.dispatch(ApplicationActions.setMessage(resp.data));
          });
          // You should config your own store provider when using Redux container component.
          return <PlaceHolder><Provider store={store}><ReduxContainerComponent /></Provider></PlaceHolder>
        },
      },
      config: {
        extraTabs: [
          {
            title: 'Extra tab',
            render: (props) => {
              // Return class component
              return <PlaceHolder><OrderDetailTab {...props} /></PlaceHolder>
            },
          },
        ],
      },
    },

    /**
     * Config page at route '/createorder'
     */
    createOrder: {
      extendUI: {
        topPlaceHolder: props => {
          return <PlaceHolder>
            CreateOrder Top Placeholder. MarketId: {props.MarketId}, ContactId: {props.CustomerId}
          </PlaceHolder>
        },
        bottomPlaceHolder: props => <PlaceHolder>CreateOrder Bottom Placeholder</PlaceHolder>,
      },
    },

    /**
     * Config page at route '/carts'
     */
    cartList: {
      extendUI: {
        topPlaceHolder: () => <PlaceHolder>Carts Top Placeholder</PlaceHolder>,
        bottomPlaceHolder: () => <PlaceHolder>Carts Bottom Placeholder</PlaceHolder>,
      },
    },

    /**
     * Config page at route '/carts/:id'
     */
    cartDetail: {
      extendUI: {
        topPlaceHolder: (props) => <PlaceHolder>CartDetail Top Placeholder: {props.Id}</PlaceHolder>,
        bottomPlaceHolder: (props) => <PlaceHolder>CartDetail Bottom Placeholde: {props.Id}</PlaceHolder>,
      },
      config: {
        extraTabs: [
          {
            title: 'Extra tab',
            render: (props) => {
              return <PlaceHolder>Tab content for cart: {props.Id}</PlaceHolder>;
            },
          },
        ],
      },
    },

    /**
     * Config page at route '/createPaymentplan'
     */
    createPaymentPlan: {
      extendUI: {
        topPlaceHolder: props => (
          <PlaceHolder>
            CreatePaymentPlan Top Placeholder. MarketId: {props.MarketId}, ContactId: {props.CustomerId}
          </PlaceHolder>
        ),
        bottomPlaceHolder: props => <PlaceHolder>CreatePaymentPlan Bottom Placeholder</PlaceHolder>,
      },
    },
    
    /**
     * Config page at route '/paymentplans'
     */
    paymentPlanList: {
      extendUI: {
        topPlaceHolder: () => <PlaceHolder>PaymentPlans Top Placeholder</PlaceHolder>,
        bottomPlaceHolder: () => <PlaceHolder>PaymentPlans Bottom Placeholder</PlaceHolder>,
      },
    },
    
    /**
     * Config page at route '/paymentplans/:id'
     */
    paymentPlanDetail: {
      extendUI: {
        topPlaceHolder: (data) => <PlaceHolder>PaymentPlanDetail Top Placeholder</PlaceHolder>,
        bottomPlaceHolder: (data) => <PlaceHolder>PaymentPlanDetail Bottom Placeholder</PlaceHolder>,
      },
      config: {
        extraTabs: [
          {
            title: 'Extra tab',
            render: (props) => {
              return <PlaceHolder>Tab content for paymentplan: {props.Id}</PlaceHolder>;
            },
          },
        ],
      },
    },

    /**
     * Config page at route '/customers/:id'
     */
    contactDetail: {
      config: {
        extraPages: [
          {
            title: 'Extra page',
            // link pattern, default path has prefix route /customers/{id}, you cannot change this prefix
            path: '/extra-page',
            exact: true,
            getLink: () => {
              return 'extra-page';
            },

            /**
             * Return JSX.Element
             */
            render: (props) => {
              return <PlaceHolder>Extra page content for customer: {props.CustomerId}</PlaceHolder>;
            },
          },
        ],
      },
    },

    /**
     * Register more routes
     */
    extraPages: [
      {
        path: '/demo-page',
        exact: true,
        render: () => {
          return <PlaceHolder>Demo page content</PlaceHolder>;
        },
      },
    ],
  }
} as AppSettingState;

