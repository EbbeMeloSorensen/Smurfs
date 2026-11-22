import React, { useEffect } from 'react';
import { Container } from 'semantic-ui-react';
import NavBar from './NavBar';
import SmurfDashboard from '../../features/smurfs/dashboard/SmurfDashboard';
import { observer } from 'mobx-react-lite';
import { Redirect, Route, Switch, useLocation } from 'react-router-dom';
import HomePage from '../../features/home/HomePage';
import SmurfForm from '../../features/smurfs/form/SmurfForm';
import SmurfDetails from '../../features/smurfs/details/SmurfDetails';
import TestErrors from '../../features/errors/TestError';
import { ToastContainer } from 'react-toastify';
import NotFound from '../../features/errors/NotFound';
import ServerError from '../../features/errors/ServerError';
import { useStore } from '../stores/store';
import LoadingComponent from './LoadingComponents';
import ModalContainer from '../common/modals/ModalContainer';
import PrivateRoute from './PrivateRoute';

function App() {
  const location = useLocation();
  const {commonStore, userStore} = useStore();

  useEffect(() => {
    if (commonStore.token) {
      userStore.getUser().finally(() => commonStore.setAppLoaded());
    } else {
      commonStore.setAppLoaded();
    }
  }, [commonStore, userStore])

  if (!commonStore.appLoaded) return <LoadingComponent content='Loading app...' />

  return (
    <>
      <ToastContainer position='bottom-right' hideProgressBar />
      <ModalContainer />
      <Route exact path='/' component={HomePage} />
      <Route
        path={'/(.+)'}
        render={() => (
          <>
            <NavBar />
            <Container style={{marginTop: '7em'}}>
              <Switch>
                <PrivateRoute exact path='/smurfs' component={SmurfDashboard} />
                <PrivateRoute key={location.key} path='/smurfs/new' component={SmurfForm} />
                <PrivateRoute key={location.key} path='/smurfs/:id/edit' component={SmurfForm} />
                <PrivateRoute path='/smurfs/:id' component={SmurfDetails} />
                <Route exact path='/people' render={() => <Redirect to='/smurfs' />} />
                <Route path='/people/:id' render={({ match }) => <Redirect to={'/smurfs/' + match.params.id} />} />
                <Route path='/manage/:id' render={({ match }) => <Redirect to={'/smurfs/' + match.params.id + '/edit'} />} />
                <Route path={['/createPerson', '/createSmurf']} render={() => <Redirect to='/smurfs/new' />} />
                <PrivateRoute path='/errors' component={TestErrors} />
                <Route path='/server-error' component={ServerError} />
                <Route component={NotFound} />
              </Switch>
            </Container>
          </>
        )}
      />
    </>
  );
}

export default observer(App);
