package com.microsoft.kiota.serialization;

import javax.annotation.Nonnull;

public interface SerializationWriterFactory {
    @Nonnull
    String getValidContentType();
    @Nonnull
    SerializationWriter getSerializationWriter(@Nonnull final String contentType);
}