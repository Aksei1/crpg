<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core';
import {
  clanTagMinLength,
  clanTagMaxLength,
  clanNameMinLength,
  clanNameMaxLength,
  clanBannerKeyMaxLength,
  clanDescriptionMaxLength,
} from '@root/data/constants.json';
import { type Clan } from '@/models/clan';
import { Language } from '@/models/language';

import {
  required,
  url,
  minLength,
  maxLength,
  clanTagPattern,
  clanBannerKeyPattern,
  discordLinkPattern,
  minValue,
  integer,
} from '@/services/validators-service';
import { notify, NotificationType } from '@/services/notification-service';
import { t } from '@/services/translate-service';
import { Region } from '@/models/region';
import { daysToMs, parseTimestamp } from '@/utils/date';

const props = withDefaults(
  defineProps<{
    clanId?: number;
    clan?: Omit<Clan, 'id'>;
  }>(),
  {
    clan: () => ({
      region: Region.Eu,
      languages: [],
      primaryColor: '#000000',
      secondaryColor: '#000000',
      name: '',
      tag: '',
      bannerKey: '',
      discord: null,
      description: '',
      armoryTimeout: daysToMs(3),
    }),
  }
);

const emit = defineEmits<{
  (e: 'submit', form: Omit<Clan, 'id'>): void;
}>();

const clanFormModel = ref<Omit<Clan, 'id'>>(props.clan);

const $v = useVuelidate(
  {
    name: {
      required,
      minLength: minLength(clanNameMinLength),
      maxLength: maxLength(clanNameMaxLength),
    },
    tag: {
      required,
      minLength: minLength(clanTagMinLength),
      maxLength: maxLength(clanTagMaxLength),
      clanTagPattern,
    },
    description: {
      maxLength: maxLength(clanDescriptionMaxLength),
    },
    bannerKey: {
      required,
      maxLength: maxLength(clanBannerKeyMaxLength),
      clanBannerKeyPattern,
    },
    discord: {
      url,
      discordLinkPattern,
    },
    armoryTimeout: {
      minValue: minValue(1),
      integer,
    },
  },
  clanFormModel
);

const onSubmit = async () => {
  if (!(await $v.value.$validate())) {
    notify(t('form.validate.invalid'), NotificationType.Warning);
    return;
  }

  emit('submit', {
    ...clanFormModel.value,
    discord: clanFormModel.value.discord === '' ? null : clanFormModel.value.discord,
  });
};
</script>

<template>
  <form @submit.prevent="onSubmit" data-aq-clan-form>
    <div class="mb-8 space-y-4">
      <FormGroup icon="hash" :label="$t('clan.update.form.field.mainInfo')">
        <div class="grid grid-cols-2 gap-4">
          <OField
            v-bind="{
              ...($v.name.$error && {
                variant: 'danger',
                message: $v.name.$errors[0].$message,
              }),
            }"
            data-aq-clan-form-field="name"
          >
            <OInput
              v-model="clanFormModel.name"
              type="text"
              hasCounter
              size="sm"
              expanded
              :placeholder="$t('clan.update.form.field.name')"
              :minlength="clanNameMinLength"
              :maxlength="clanNameMaxLength"
              data-aq-clan-form-input="name"
              @blur="$v.name.$touch"
              @focus="$v.name.$reset"
            />
          </OField>

          <OField
            v-bind="{
              ...($v.tag.$error && {
                variant: 'danger',
                message: $v.tag.$errors[0].$message,
              }),
            }"
            data-aq-clan-form-field="tag"
          >
            <OInput
              v-model="clanFormModel.tag"
              type="text"
              hasCounter
              size="sm"
              expanded
              :placeholder="$t('clan.update.form.field.tag')"
              :minLength="clanTagMinLength"
              :maxlength="clanTagMaxLength"
              data-aq-clan-form-input="tag"
              @blur="$v.tag.$touch"
              @focus="$v.tag.$reset"
            />
          </OField>

          <OField
            class="col-span-2"
            v-bind="{
              ...($v.description.$error && {
                variant: 'danger',
                message: $v.description.$errors[0].$message,
              }),
            }"
            data-aq-clan-form-field="description"
          >
            <OInput
              v-model="clanFormModel.description"
              :placeholder="`${$t('clan.update.form.field.description')} (${$t(
                'form.field.optional'
              )})`"
              type="textarea"
              rows="5"
              hasCounter
              size="sm"
              expanded
              :maxlength="clanDescriptionMaxLength"
              data-aq-clan-form-input="description"
              @blur="$v.description.$touch"
              @focus="$v.description.$reset"
            />
          </OField>
        </div>
      </FormGroup>

      <FormGroup icon="region" :label="$t('region-title')">
        <div class="space-y-8">
          <OField :addons="false">
            <div class="flex flex-col gap-4">
              <ORadio
                v-for="region in Object.keys(Region)"
                v-model="clanFormModel.region"
                :native-value="region"
                data-aq-clan-form-input="region"
              >
                {{ $t(`region.${region}`, 0) }}
              </ORadio>
            </div>
          </OField>

          <OField>
            <VDropdown :triggers="['click']">
              <template #default="{ shown }">
                <OButton variant="secondary" outlined size="lg">
                  {{ $t('clan.update.form.field.languages') }}
                  <div class="flex items-center gap-1.5">
                    <Tag
                      v-for="l in clanFormModel.languages"
                      :label="l"
                      v-tooltip="$t(`language.${l}`)"
                      variant="primary"
                    />
                  </div>
                  <Divider inline />
                  <OIcon
                    icon="chevron-down"
                    size="lg"
                    :rotation="shown ? 180 : 0"
                    class="text-content-400"
                  />
                </OButton>
              </template>

              <template #popper>
                <div class="max-h-64 max-w-md overflow-y-auto">
                  <DropdownItem v-for="l in Object.keys(Language)">
                    <OCheckbox
                      v-model="clanFormModel.languages"
                      :nativeValue="l"
                      class="items-center"
                      :label="$t(`language.${l}`) + ` - ${l}`"
                    />
                  </DropdownItem>
                </div>
              </template>
            </VDropdown>
          </OField>
        </div>
      </FormGroup>

      <FormGroup>
        <template #label>
          <ClanTagIcon :color="clanFormModel.primaryColor" size="lg" />
          {{ $t('clan.update.form.field.colors') }}
        </template>
        <div class="grid grid-cols-2 gap-4">
          <OField :label="`${$t('clan.update.form.field.primaryColor')}:`" horizontal>
            <div class="text-content-100">{{ clanFormModel.primaryColor }}</div>
            <OInput
              type="color"
              v-model="clanFormModel.primaryColor"
              data-aq-clan-form-input="primaryColor"
            />
          </OField>

          <OField :label="`${$t('clan.update.form.field.secondaryColor')}:`" horizontal>
            <div class="text-content-100">{{ clanFormModel.secondaryColor }}</div>
            <OInput
              type="color"
              v-model="clanFormModel.secondaryColor"
              data-aq-clan-form-input="secondaryColor"
            />
          </OField>
        </div>
      </FormGroup>

      <FormGroup icon="banner" :label="$t('clan.update.form.field.bannerKey')">
        <OField
          v-bind="{
            ...($v.bannerKey.$error && {
              variant: 'danger',
            }),
          }"
          data-aq-clan-form-field="bannerKey"
        >
          <template #message>
            <template v-if="$v.bannerKey.$error">{{ $v.bannerKey.$errors[0].$message }}</template>
            <template v-else>
              <i18n-t scope="global" keypath="clan.update.bannerKeyGeneratorTools" tag="div">
                <template #link>
                  <a
                    href="https://bannerlord.party"
                    target="_blank"
                    class="text-content-link hover:text-content-link-hover"
                  >
                    bannerlord.party
                  </a>
                </template>
              </i18n-t>
            </template>
          </template>

          <OInput
            v-model="clanFormModel.bannerKey"
            hasCounter
            expanded
            size="sm"
            :maxlength="clanBannerKeyMaxLength"
            data-aq-clan-form-input="bannerKey"
            @blur="$v.bannerKey.$touch"
            @focus="$v.bannerKey.$reset"
          />
        </OField>
      </FormGroup>

      <FormGroup icon="discord" :label="$t('clan.update.form.field.discord')">
        <OField
          v-bind="{
            ...($v.discord.$error && {
              variant: 'danger',
              message: $v.discord.$errors[0].$message,
            }),
          }"
          data-aq-clan-form-field="discord"
        >
          <OInput
            v-model="clanFormModel.discord"
            type="text"
            size="sm"
            expanded
            :placeholder="`${$t('clan.update.form.field.discord')} (${$t('form.field.optional')})`"
            data-aq-clan-form-input="discord"
            @blur="$v.discord.$touch"
            @focus="$v.discord.$reset"
          />
        </OField>
      </FormGroup>

      <FormGroup icon="armory" :label="$t('clan.update.form.group.armory.label')">
        <div class="grid grid-cols-2 gap-4">
          <OField
            data-aq-clan-form-field="armoryTimeout"
            :label="$t('clan.update.form.group.armory.field.armoryTimeout.label')"
            :message="$t('clan.update.form.group.armory.field.armoryTimeout.hint')"
            v-bind="{
              ...($v.armoryTimeout.$error && {
                variant: 'danger',
                message: $v.armoryTimeout.$errors[0].$message,
              }),
            }"
          >
            <OInput
              :modelValue="parseTimestamp(clanFormModel.armoryTimeout).days"
              @update:modelValue="
                (days: string) => (clanFormModel.armoryTimeout = daysToMs(Number(days)))
              "
              type="number"
              size="sm"
              expanded
              data-aq-clan-form-input="armoryTimeout"
              @blur="$v.armoryTimeout.$touch"
              @focus="$v.armoryTimeout.$reset"
            />
          </OField>
        </div>
      </FormGroup>
    </div>

    <div class="flex items-center justify-center gap-4">
      <template v-if="clanId === undefined">
        <OButton
          nativeType="submit"
          variant="primary"
          size="xl"
          :label="$t('action.create')"
          data-aq-clan-form-action="create"
        />
      </template>

      <template v-else>
        <RouterLink
          :to="{ name: 'ClansId', params: { id: clanId } }"
          data-aq-clan-form-action="cancel"
        >
          <OButton variant="primary" outlined size="xl" :label="$t('action.cancel')" />
        </RouterLink>
        <OButton
          variant="primary"
          size="xl"
          :label="$t('action.save')"
          nativeType="submit"
          data-aq-clan-form-action="save"
        />
      </template>
    </div>
  </form>
</template>
