import React from 'react'
import PropTypes from 'prop-types'

const DeadlineWarningIcon = ({
    width,
    height,
}) => (
    <svg
        height={height}
        viewBox="0 0 24 24"
        width={width}
        xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M0 0h24v24H0z" fill="none"/>
        <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"/>
    </svg>
);

DeadlineWarningIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired
};

export default DeadlineWarningIcon