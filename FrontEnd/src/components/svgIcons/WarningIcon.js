import React from 'react'
import PropTypes from 'prop-types'

const WarningIcon = ({
    width,
    height
}) => (
    <svg
        fill="#fff"
        height={height}
        width={width}
        viewBox="0 0 24 24"
        xmlns="http://www.w3.org/2000/svg"
    >
        <circle cx="12" cy="19" r="2"/>
        <path d="M10 3h4v12h-4z"/>
        <path d="M0 0h24v24H0z" fill="none"/>
    </svg>
);

WarningIcon.propTypes = {
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired
};


export default WarningIcon