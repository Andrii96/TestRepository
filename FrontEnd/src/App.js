import React, { Component, Fragment } from 'react'
import { Route, BrowserRouter, Switch, Redirect } from 'react-router-dom'
import PropTypes from 'prop-types'

import List from './components/List'
import RecordView from './components/RecordView'
import NotFoundPage from './components/NotFoundPage'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import * as currentUserActions from './actions/currentUserActions'
import { intlShape, injectIntl } from 'react-intl'
import generalMessages from './i18n/generalMessages'
import ReduxToastr from 'react-redux-toastr'

class App extends Component {
    componentDidMount() {
        const {currentUserActions, uid, locale} = this.props;

        currentUserActions.loadCurrentUser(uid, locale);
    }

    render() {
        const {currentUser: {ContactId}, intl: {formatMessage}} = this.props;

        return (
            <Fragment>
                {!ContactId && <h3 style={{display: 'flex', alignItems: 'center', margin: '0 0 15px'}}>
                    <img
                        src="/img/loading.gif"
                        className="tl-loading-gif"
                        alt={formatMessage(generalMessages.loading)}
                        style={{marginRight: 10}}
                    />
                    {formatMessage(generalMessages.loading)}
                </h3>}
                {ContactId && <BrowserRouter>
                    <Switch>
                        {(process.env.NODE_ENV === 'development' || window.location.host === '193.93.216.233:8072') && <Redirect exact from='/' to='/en_US/validate/index/list'/>}
                        <Route exact path="/:lang/validate/index/list" component={List}/>
                        <Route exact path="/:lang/validate/index/view/id/:id" component={List}/>
                        <Route exact path="/:lang/validate/orc" component={RecordView}/>
                        <Route component={NotFoundPage} />
                    </Switch>
                </BrowserRouter>}
                <ReduxToastr
                    timeOut={10000}
                    position="top-center"
                    newestOnTop={false}
                    transitionIn="fadeIn"
                    transitionOut="fadeOut"
                />
            </Fragment>
        );
    }
}

App.propTypes = {
    currentUserActions: PropTypes.object.isRequired,
    currentUser: PropTypes.object.isRequired,
    locale: PropTypes.string.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        currentUser: state.currentUser,
        locale: state.config.locale
    }
}

function mapDispatchToProps(dispatch) {
    return {
        currentUserActions: bindActionCreators(currentUserActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(App));
