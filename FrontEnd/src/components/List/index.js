import React, { Component, Fragment } from 'react'
import ReactDOM from 'react-dom'
import PropTypes from 'prop-types'
import { connect } from 'react-redux'
import { bindActionCreators } from 'redux'
import { intlShape, injectIntl } from 'react-intl'

import './List.css'
import ValidationList from '../ValidationList'
import TaskFilters from '../TaskFilters'

import * as filesActions from '../../actions/filesActions'

class List extends Component {
    constructor(props) {
        super(props);
        this.state = {

        };

        this.onInputUploadChange = this.onInputUploadChange.bind(this);
        this.importFile = this.importFile.bind(this);
    }
    
    importFile() {
        const {filesActions} = this.props;
        const {importFormData} = this.state;

        if (!importFormData) return this.setState({noFileChosen: true});

        const fileUploadDom = ReactDOM.findDOMNode(this.refs.fileUpload);
        this.setState({startImporting: true});

        filesActions.importFile(importFormData, {taskId: 1})
            .then(res => {
                if (fileUploadDom) fileUploadDom.value = '';
                this.setState({startImporting: false, importFormData: null});
            }).catch(e => {
                if (fileUploadDom) fileUploadDom.value = '';
                this.setState({startImporting: false, importFormData: null});
            });
    }

    onInputUploadChange(e) {
        this.setState({noFileChosen: null});
        e.preventDefault();
        const files = e.target.files;
        if (!files.length) return;

        const formData = new FormData();

        formData.append(`file`, files[0]);

        this.setState({importFormData: formData});
    }

    render() {

        return (
            <Fragment>

                <TaskFilters />

                <ValidationList />

            </Fragment>
        );
    }
}

List.propTypes = {
    filesActions: PropTypes.object.isRequired,
    filesList: PropTypes.array.isRequired,
    intl: intlShape.isRequired
};

function mapStateToProps(state) {
    return {
        filesList: state.filesList
    }
}

function mapDispatchToProps(dispatch) {
    return {
        filesActions: bindActionCreators(filesActions, dispatch)
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(injectIntl(List))
