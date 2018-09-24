import React, {Component} from 'react'
import { intlShape, injectIntl, FormattedMessage } from 'react-intl'

import './NotFoundPage.css';

class NotFoundPage extends Component {
    constructor(props) {
        super(props);

        this.state = {

        }

    }

    render() {

        return (
            <div className="tl-text-center">
                <FormattedMessage
                    id="notFoundPage.message404"
                    defaultMessage="Page not found"
                    tagName="h1"
                />
            </div>
        );
    }
}

NotFoundPage.propTypes = {
    intl: intlShape.isRequired
};

export default injectIntl(NotFoundPage);
